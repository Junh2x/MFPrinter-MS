const { initializeApp } = require("firebase-admin/app");
const { getFirestore, FieldValue } = require("firebase-admin/firestore");
const { setGlobalOptions } = require("firebase-functions/v2");
const { onRequest } = require("firebase-functions/v2/https");

setGlobalOptions({
  maxInstances: 10,
  region: "asia-northeast3",
  serviceAccount: "firebase-adminsdk-fbsvc@scanlink-d0ed5.iam.gserviceaccount.com",
});

initializeApp();
const db = getFirestore("auth-code");

const ADMIN_KEY = process.env.ADMIN_KEY || "";

function handleCors(req, res) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Methods", "GET, POST, PATCH, DELETE, OPTIONS");
  res.set("Access-Control-Allow-Headers", "Content-Type, X-Admin-Key");
  if (req.method === "OPTIONS") {
    res.status(204).send("");
    return true;
  }
  return false;
}

// 기존 memo 필드를 companyName으로 폴백 매핑하여 점진 마이그레이션
function toLicenseDto(doc) {
  const d = doc.data();
  return {
    id: doc.id,
    companyType: d.companyType || "",
    companyName: d.companyName || d.memo || "",
    code: d.code,
    status: d.status || "active",
    createdAt: d.createdAt?.toDate?.() || null,
    lastUsedAt: d.lastUsedAt?.toDate?.() || null,
  };
}

// ===== 인증 코드 검증 (공개) =====
exports.verify = onRequest({ invoker: "public" }, async (req, res) => {
  if (handleCors(req, res)) return;
  if (req.method !== "POST") {
    return res.status(405).json({ valid: false, message: "Method not allowed" });
  }

  const { code } = req.body || {};
  if (!code || typeof code !== "string") {
    return res.status(400).json({ valid: false, message: "인증 코드를 입력해주세요." });
  }

  const trimmed = code.trim().toUpperCase();
  const snapshot = await db
    .collection("licenses")
    .where("code", "==", trimmed)
    .limit(1)
    .get();

  if (snapshot.empty) {
    return res.json({ valid: false, message: "유효하지 않은 인증 코드입니다." });
  }

  const doc = snapshot.docs[0];
  const data = doc.data();

  if (data.status === "revoked") {
    return res.json({ valid: false, message: "비활성화된 인증 코드입니다." });
  }

  await doc.ref.update({
    status: "active",
    lastUsedAt: FieldValue.serverTimestamp(),
  });

  return res.json({ valid: true, message: "인증 성공" });
});

// ===== 관리자 API (ADMIN_KEY 필요) =====
exports.licenses = onRequest({ invoker: "public" }, async (req, res) => {
  if (handleCors(req, res)) return;

  if (!ADMIN_KEY || req.headers["x-admin-key"] !== ADMIN_KEY) {
    return res.status(401).json({ error: "인증 실패" });
  }

  switch (req.method) {
    case "GET": {
      const snapshot = await db
        .collection("licenses")
        .orderBy("createdAt", "desc")
        .get();
      const licenses = snapshot.docs.map(toLicenseDto);
      return res.json({ licenses });
    }

    case "POST": {
      const { companyType, companyName, code } = req.body || {};
      const userProvidedCode =
        typeof code === "string" && code.trim() ? code.trim().toUpperCase() : null;

      // 사용자 입력 코드는 1회, 자동 생성은 충돌 시 최대 5회 재시도
      const maxAttempts = userProvidedCode ? 1 : 5;

      for (let attempt = 0; attempt < maxAttempts; attempt++) {
        const licenseCode = userProvidedCode || generateCode();
        try {
          const created = await db.runTransaction(async (t) => {
            const dup = await t.get(
              db.collection("licenses").where("code", "==", licenseCode).limit(1)
            );
            if (!dup.empty) throw new Error("DUPLICATE");

            const newRef = db.collection("licenses").doc();
            t.set(newRef, {
              companyType: companyType || "",
              companyName: companyName || "",
              code: licenseCode,
              status: "active",
              createdAt: FieldValue.serverTimestamp(),
              lastUsedAt: null,
            });
            return { id: newRef.id, code: licenseCode };
          });
          return res.status(201).json(created);
        } catch (e) {
          if (e.message === "DUPLICATE") {
            if (userProvidedCode) {
              return res.status(409).json({ error: "이미 존재하는 코드입니다." });
            }
            continue; // 자동 생성 충돌 → 재시도
          }
          throw e;
        }
      }
      return res
        .status(500)
        .json({ error: "코드 생성에 실패했습니다. 다시 시도해주세요." });
    }

    case "PATCH": {
      const id = req.query.id;
      if (!id) {
        return res.status(400).json({ error: "ID가 필요합니다." });
      }

      const { companyType, companyName, code, status } = req.body || {};

      // 입력 사전 검증
      if (typeof code === "string" && !code.trim()) {
        return res.status(400).json({ error: "코드는 비워둘 수 없습니다." });
      }
      if (typeof status === "string" && status !== "active" && status !== "revoked") {
        return res.status(400).json({ error: "상태 값이 올바르지 않습니다." });
      }

      const docRef = db.collection("licenses").doc(id);

      try {
        await db.runTransaction(async (t) => {
          const snap = await t.get(docRef);
          if (!snap.exists) throw new Error("NOT_FOUND");

          const updates = {};
          if (typeof companyType === "string") updates.companyType = companyType;
          if (typeof companyName === "string") updates.companyName = companyName;
          if (typeof status === "string") updates.status = status;

          if (typeof code === "string") {
            const newCode = code.trim().toUpperCase();
            if (newCode !== snap.data().code) {
              const dup = await t.get(
                db.collection("licenses").where("code", "==", newCode).limit(1)
              );
              if (!dup.empty && dup.docs[0].id !== id) {
                throw new Error("DUPLICATE");
              }
            }
            updates.code = newCode;
          }

          if (Object.keys(updates).length === 0) throw new Error("NO_FIELDS");

          t.update(docRef, updates);
        });

        const after = await docRef.get();
        return res.json(toLicenseDto(after));
      } catch (e) {
        if (e.message === "NOT_FOUND")
          return res.status(404).json({ error: "존재하지 않는 라이선스입니다." });
        if (e.message === "DUPLICATE")
          return res.status(409).json({ error: "이미 존재하는 코드입니다." });
        if (e.message === "NO_FIELDS")
          return res.status(400).json({ error: "변경할 필드가 없습니다." });
        throw e;
      }
    }

    case "DELETE": {
      const id = req.query.id;
      if (!id) {
        return res.status(400).json({ error: "ID가 필요합니다." });
      }
      await db.collection("licenses").doc(id).delete();
      return res.json({ success: true });
    }

    default:
      return res.status(405).json({ error: "Method not allowed" });
  }
});

function generateCode() {
  const chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
  let code = "";
  for (let i = 0; i < 16; i++) {
    if (i > 0 && i % 4 === 0) code += "-";
    code += chars[Math.floor(Math.random() * chars.length)];
  }
  return code;
}
