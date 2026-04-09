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
  res.set("Access-Control-Allow-Methods", "GET, POST, DELETE, OPTIONS");
  res.set("Access-Control-Allow-Headers", "Content-Type, X-Admin-Key");
  if (req.method === "OPTIONS") {
    res.status(204).send("");
    return true;
  }
  return false;
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

  if (data.expiresAt && data.expiresAt.toDate() < new Date()) {
    await doc.ref.update({ status: "expired" });
    return res.json({ valid: false, message: "만료된 인증 코드입니다." });
  }

  if (data.status === "expired") {
    return res.json({ valid: false, message: "만료된 인증 코드입니다." });
  }

  await doc.ref.update({
    status: "active",
    lastUsedAt: FieldValue.serverTimestamp(),
    ...(data.activatedAt ? {} : { activatedAt: FieldValue.serverTimestamp() }),
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
      const licenses = snapshot.docs.map((doc) => {
        const d = doc.data();
        return {
          id: doc.id,
          code: d.code,
          status: d.status,
          memo: d.memo || "",
          createdAt: d.createdAt?.toDate?.() || null,
          activatedAt: d.activatedAt?.toDate?.() || null,
          lastUsedAt: d.lastUsedAt?.toDate?.() || null,
          expiresAt: d.expiresAt?.toDate?.() || null,
        };
      });
      return res.json({ licenses });
    }

    case "POST": {
      const { code, memo, expiresAt } = req.body || {};
      const licenseCode = (code || generateCode()).toUpperCase();

      const existing = await db
        .collection("licenses")
        .where("code", "==", licenseCode)
        .limit(1)
        .get();
      if (!existing.empty) {
        return res.status(409).json({ error: "이미 존재하는 코드입니다." });
      }

      const newLicense = {
        code: licenseCode,
        status: "active",
        memo: memo || "",
        createdAt: FieldValue.serverTimestamp(),
        activatedAt: null,
        lastUsedAt: null,
        expiresAt: expiresAt ? new Date(expiresAt) : null,
      };

      const docRef = await db.collection("licenses").add(newLicense);
      return res.status(201).json({ id: docRef.id, code: licenseCode });
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
