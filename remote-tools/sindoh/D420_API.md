## 박스 목록록
Request URL
http://192.168.11.241/wcd/box_list.xml
Request Method
GET
Status Code
200 OK
페이로드:
응답:<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=yes">
        <meta content="text/javascript" http-equiv="Content-Script-Type">
        <script type="text/javascript" charset="utf-8">
            function getFlashVars_Status() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User&ExternalIfMode=None"
            }
            function getFlashVars_InputTray() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User"
            }
            function getFlashVars_ActiveJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User&Favorite=" + Favorite + ""
            }
            function getFlashVars_DoneJob() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User"
            }
            function getFlashVars_Commlist() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User"
            }
            function getFlashVars_AccumulationJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=5&UserMode=User&Favorite=" + Favorite + ""
            }
            function setLocation(url) {
                job_f.btnDisabled();
                location.href = url;
            }
            function setFavoriteLocation(url) {
                parent.MyTab.PortalMain.prototype.getAjaxProc(url);
            }
            function callSetProcWidget() {
                parent.MyTab.PortalMain.prototype.setProcWidget();
            }
            function callHideWait() {
                parent.MyTab.PortalMain.prototype.HideWait();
            }
            function getFlashVars_LangFileName() {
                return {
                    lang: "lang_fl_Ko.xml",
                    comLang: "lang_co_Ko.xml",
                    jobLang: "lang_bo_Ko.xml"
                };
            }
        </script>
        <script type="text/javascript" charset="utf-8">
            function getFlashVars_Box(str) {
                var FuncID = box_f.getFuncID();
                var SelectedBoxID = 0;
                var Attested = 0;
                var Favorite = 0;
                var BoxFlashDebug = "Off";
                var BoxFlashTrace = "Off";
                var HddInstalled = parent.document.getElementById("HddInst").value;
                var cookieSn = box_f.getCookieData("sn");
                if (!parent.document.getElementById("H_FAV_FLG")) {
                    if (parent.document.getElementById("SelectedBoxID")) {
                        SelectedBoxID = parent.document.getElementById("SelectedBoxID").value;
                        Attested = parent.document.getElementById("Attested").value;
                        BoxFlashDebug = parent.document.getElementById("BoxFlashDebug").value;
                        BoxFlashTrace = parent.document.getElementById("BoxFlashTrace").value;
                    } else if (parent.document.getElementById("SelectedBoxID2")) {
                        SelectedBoxID = parent.document.getElementById("SelectedBoxID2").value;
                        Attested = parent.document.getElementById("Attested2").value;
                        BoxFlashDebug = parent.document.getElementById("BoxFlashDebug2").value;
                        BoxFlashTrace = parent.document.getElementById("BoxFlashTrace2").value;
                    }
                } else {
                    if (str == "User") {
                        FuncID = "F_UOU";
                        SelectedBoxID = parent.document.getElementById("SelectedBoxID").value;
                        Attested = parent.document.getElementById("Attested").value;
                        BoxFlashDebug = parent.document.getElementById("BoxFlashDebug").value;
                        BoxFlashTrace = parent.document.getElementById("BoxFlashTrace").value;
                    } else {
                        FuncID = "F_SOU";
                        SelectedBoxID = parent.document.getElementById("SelectedBoxID2").value;
                        Attested = parent.document.getElementById("Attested2").value;
                        BoxFlashDebug = parent.document.getElementById("BoxFlashDebug2").value;
                        BoxFlashTrace = parent.document.getElementById("BoxFlashTrace2").value;
                    }
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_bo_Ko.xml&LoginMode=PublicUser&DN35B3=Off&DN35B4=Off&LangNo=Ko&FuncID=" + FuncID + "&SelectedBoxID=" + SelectedBoxID + "&Attested=" + Attested + "&BoxFlashDebug=" + BoxFlashDebug + "&BoxFlashTrace=" + BoxFlashTrace + "&HddInstalled=" + HddInstalled + "&Favorite=" + Favorite + "&cookieSn=" + cookieSn;
            }
            function setCookieBoxFlash(str) {
                document.cookie = "sn=" + str + ";expires=Tue, 1-Jan-2030 00:00:00 GMT;";
            }
            function getCookieBoxFlash() {
                var strCookie = document.cookie + ";";
                var intStart = strCookie.indexOf("sn=");
                var intEnd = 0;
                var strData = "";
                if (intStart != -1) {
                    intEnd = strCookie.indexOf(";", intStart);
                    strData = unescape(strCookie.substring(intStart + 3, intEnd));
                }
                return strData;
            }
            function setLocation(url) {
                box_f.btnDisabled();
                location.href = url;
            }
            function setFavoriteLocation(url) {
                parent.document.getElementById("BoxFlashUpdate").value = "true";
                parent.MyTab.PortalMain.prototype.getAjaxProc(url);
            }
            function callSetProcWidget() {
                parent.MyTab.PortalMain.prototype.setProcWidget();
            }
            function callHideWait() {
                parent.MyTab.PortalMain.prototype.HideWait();
            }
            function setCookieOperate(sOperation) {
                box_f.setCookie(sOperation);
            }
            function setCookieUser(sCookie) {
                box_f.setCookie(sCookie);
            }
        </script>
        <link rel="stylesheet" type="text/css" href="default_blackwhite_skin.css">
        <link rel="stylesheet" type="text/css" href="box.css">
        <link rel="stylesheet" type="text/css" href="default_user_skin.css">
        <style type="text/css" id="tempCssId"></style>
        <title>박스</title>
    </head>
    <body>
        <input type="hidden" id="document_id" value="box">
        <div id="Top" class="top-layout" style="display:none">
            <input type="hidden" id="PCM_FUNC_VER" value="10">
            <a href="#TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭으로 링크">
            </a>
            <div class="main-layout-top">
                <div class="main-layout-top-upper">
                    <div class="corp-logo">
                        <img src="horizon_s_SINDOH.gif" width="168" height="37" border="0" alt=" " class="corplogo-image">
                    </div>
                    <div class="topbutton-layout" cellpadding="0" cellspacing="0">
                        <div class="btn-logout">
                            <input type="button" class="logout-button2 btn-style-logout" id="Logout_Button" ALIGN="left" onclick="html_f.closeDialogAll(); html_f.showLogout();" value="로그인 화면">
                            <img src="login.png" alt=" " width="36" height="36" onclick="html_f.closeDialogAll(); html_f.showLogout();">
                        </div>
                        <div class="btn-change-password"></div>
                    </div>
                    <div class="logininfo">
                        <div class="logininfo-icon-layout" onclick="html_f.toggleMenuList(this, 'loginInfo-text', 'toggle-show');">
                            <input type="hidden" id="H_LoginMode" value="PublicUser">
                            <img src="b-user.png" alt=" " width="36" height="36">
                        </div>
                        <div id="loginInfo-text" class="logininfo-layout loginInfo-text" cellpadding="0" cellspacing="0">
                            <input type="hidden" id="loginMode" value="공유">
                            공유<input type="hidden" id="loginNameHidden" value="Public">
                        </div>
                    </div>
                </div>
                <div class="main-layout-top-bottom">
                    <div class="header-img">
                        <div class="tool-logo">
                            <img src="webconnection3_SINDOH.gif" width="189" height="25" border="0" alt=" ">
                        </div>
                        <div class="devicename-layout sindoh-layout" cellpadding="0" cellspacing="0">SINDOH D420</div>
                    </div>
                    <div class="contents-clear flash-layout">
                        <input type="hidden" name="PswcForm" id="PswcForm" value="">
                        <div id="StatusFlash" style="display:none;">
                            <div id="StatusFlashOn" style="display:none;"></div>
                            <div id="StatusFlashOff" style="display:none;">최신 Flah Player를 설치하십시오.</div>
                        </div>
                        <div id="StatusHtml" style="display:none;">
                            <script type="text/javascript" language="javascript" src="websocket_common.js"></script>
                            <script type="text/javascript" language="javascript" src="websocket_status.js"></script>
                            <table width="285px" border="0" cellspacing="0" id="DI_SCAN_TABLE" style="border-bottom-width:2px; border-bottom-style:solid; border-bottom-color:#3e3e3e">
                                <tr>
                                    <td width="35" bgcolor="#3e3e3e">
                                        <img id="DI_I_SS" alt=" " width="32" height="24" border="0" src="ScanRe.png">
                                    </td>
                                    <td width="155px" bgcolor="#3e3e3e">
                                        <font id="DI_T_SS" color="white">스캔할 준비가 되었습니다</font>
                                    </td>
                                    <td width="5" id="DI_T_SN"></td>
                                    <td width="90px" id="DI_T_SJ" style="color:white"></td>
                                </tr>
                            </table>
                            <table width="285px" border="0" cellspacing="0" id="DI_PRINT_TABLE" style="display:none;">
                                <tr>
                                    <td width="35" bgcolor="#3e3e3e" id="print-status-img" color="white">
                                        <a class="link-status-image" href="javascript:html_f.closeDialogAll();%20html_f.StatusJump('S_INF')">
                                            <img id="DI_I_PS" title="" alt=" " width="32" height="24" border="0" src="EngCa.png">
                                        </a>
                                    </td>
                                    <td width="155px" bgcolor="#3e3e3e" id="print-status-txt" color="white">
                                        <a class="link-status-text" href="javascript:html_f.closeDialogAll();%20html_f.StatusJump('S_INF');">
                                            <font id="DI_T_PS" color="white" bgcolor="#3e3e3e">용지 없음</font>
                                        </a>
                                    </td>
                                    <td width="5" id="DI_T_PN"></td>
                                    <td width="90px" id="DI_T_PJ" style="color:white"></td>
                                </tr>
                            </table>
                            <div id="StatusTemplate">
                                <script type="text/realtimeUpdate-template" value="link">
                                    <a class="link-status-text" href="javascript:html_f.closeDialogAll();html_f.StatusJump('#cookieid');">#content</a>
                                </script>
                            </div>
                        </div>
                    </div>
                    <div id="header-btn" class="contents-clear header-btn">
                        <div class="header-img">
                            <div class="tool-logo">
                                <img src="webconnection3_SINDOH.gif" width="189" height="25" border="0" alt=" ">
                            </div>
                            <div class="devicename-layout sindoh-layout" cellpadding="0" cellspacing="0">SINDOH D420</div>
                        </div>
                        <div class="btn-group">
                            <div class="btn-default dropdown-btn" id="dropdownBtn" onclick="html_f.toggleMenuList(this, 'dropdownMenu', 'toggle-show');">
                                <a href="javascript:;">
                                    <img src="mfp_icon1.png" width="24" height="24" border="0" alt="MFP情報検索/登録/表示">
                                </a>
                            </div>
                            <ul class="dropdown-menu" id="dropdownMenu">
                                <li class="dropdown-item icon-search" onclick="html_f.showModal(this,'MFP_SEARCH_MODAL');html_f.resetParam();html_f.getMfpList();">
                                    <input type="button" value="주변 MFP 검색">
                                </li>
                                <li class="dropdown-item icon-register" onclick="html_f.showModal(this,'MFP_REGISTER_MODAL');html_f.changeClickEvent();">
                                    <input type="button" value="MFP 정보 등록">
                                </li>
                                <li class="dropdown-item icon-list" onclick="html_f.showModal(this,'MFP_LOGIN_MODAL');">
                                    <input type="button" value="MFP 정보 표시">
                                </li>
                            </ul>
                        </div>
                        <div class="helpicon-layout" cellpadding="0" cellspacing="0">
                            <input type="hidden" id="OnlineHelpUrl" value="">
                            <a href="" id="helprelation" target="_blank" onclick="html_f.closeDialogAll(); html_f.setHelpPram('Ko')">
                                <img src="help.png" width="23" height="22" border="0" alt="수동">
                            </a>
                        </div>
                        <div class="refreshicon-layout" cellpadding="0" cellspacing="0">
                            <a href="javascript:html_f.closeDialogAll();%20html_f.updateUserPage();">
                                <img src="btn_refresh.png" width="24" height="24" border="0" alt="새로고침">
                            </a>
                        </div>
                        <div class="attentionicon-layout" cellpadding="0" cellspacing="0">
                            <div id="HddMirroringError_area" class="HddMirroringError_area">
                                <table border="0" cellspacing="0">
                                    <tr></tr>
                                </table>
                            </div>
                            <div id="networkErr_area" class="networkErr_area">
                                <table width="24" border="0" cellspacing="0">
                                    <tr>
                                        <img alt="" width="24" height="24" border="0" src="Network_NoError1.png">
                                        <input type="hidden" id="netWorkFlag" value="true">
                                        <input type="hidden" id="kmSaasgwFlag" value="true">
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="contents-clear moblie-status">
                        <div id="status-change-btn" class="status-change-btn" onclick="html_f.toggleMenuList(this, 'header-btn', 'toggle-show');">
                            <img width="36px" height="36px" border="0" src="mfp_ellipsis.png">
                        </div>
                    </div>
                </div>
                <div class="contents-clear menu-list-menu-back">
                    <div class="menu-list-btn">
                        <a href="javascript:;" id="menuListBtn" onclick="html_f.closeDialogAll(); html_f.toggleMenuList(this, 'mainMenu','toggle-show');">
                            <img src="tab-menu-icon1.png" alt="톱 메뉴" style="display:none">톱 메뉴
                        </a>
                    </div>
                    <div class="menu-back-btn">
                        <a href="javascript:;" id="menuBackBtn" onclick="html_f.closeDialogAll(); html_f.toggleMenuList(this, 'subMenu','toggle-show');">
                            <img src="tab-menu-icon2.png" alt="Sub 메뉴" style="display:none">Sub 메뉴
                        </a>
                    </div>
                </div>
            </div>
            <a href="#TabStartLink" name="TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭">
            </a>
            <a href="#MenuStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="부 메뉴로 링크">
            </a>
        </div>
        <div class="modal" id="MFP_REGISTER_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_REGISTER_FORM_CON">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            등록 번호 ： <label id="loginNumber" class="modal-number">1</label>
                            /20MFP 장치 수
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner" id="inner-register">
                            <div id="MFP_REGISTER_FORM">
                                <form class="form-content">
                                    <table class="table-normal" id="MFP_REGISTER_TAB">
                                        <tbody>
                                            <tr>
                                                <td width="35%">등록 번호</td>
                                                <td width="65%">
                                                    <label id="loginNo" class="modal-number">1</label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>IP 주소</td>
                                                <td>
                                                    <input type="text" id="loginIp" tabindex="1">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>기종명</td>
                                                <td>
                                                    <input type="text" id="loginName" tabindex="2">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <div class="text-formerror">
                                        <div id="InputDataStr_1" style="display:none">IP 주소 : 입력하지 않았습니다.</div>
                                        <div id="InputDataStr_2" style="display:none">IP 주소 : 입력 가능 문자수 초과</div>
                                        <div id="InputDataStr_3" style="display:none">기종명 : 입력하지 않았습니다.</div>
                                        <div id="InputDataStr_4" style="display:none">기종명 : 입력 가능 문자수 초과</div>
                                        <div id="InputDataStr_11" style="display:none">IP 주소 : 무효 글자 포함.</div>
                                    </div>
                                </form>
                            </div>
                            <div class="success-inner" id="MFP_DIALOG_SUC">
                                <div class="tip-area">
                                    <p class="tip-success">등록 완료</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="submit" class="btn  btn-primary" id="MFP_REGISTER_SUBMIT_BTN" onclick="html_f.chkInputFun(this);html_f.registerSubmit();" value="확인" tabindex="3">
                        <input type="submit" class="btn  btn-primary" id="MFP_REGISTER_SUBMIT_BTN1" onclick="html_f.chkInputFun(this);html_f.registerSubmit();" value="확인" tabindex="3">
                        <input type="button" id="MFP_REGISTER_FINISH_BTN" class="btn btn-default" onclick="html_f.closeModal('MFP_REGISTER_MODAL');" value="확인" tabindex="4">
                        <input type="button" class="btn btn-primary hide" id="MFP_REGISTER_CONTINU_BTN" onclick="html_f.goOnRegister();" value="계속 등록" tabindex="4">
                        <input type="button" class="btn btn-primary hide" id="MFP_OVER_BTN" onclick="html_f.closeModal('MFP_REGISTER_MODAL');html_f.showModal('','MFP_SEARCH_MODAL');html_f.DisplayNone('MFP_LOADING');html_f.DisplayBlock('MFP_SEARCH_LIST');" value="완료" tabindex="5">
                        <input type="button" id="MFP_REGISTER_CANCEL_BTN" class="btn btn-default" onclick="html_f.closeModal('MFP_REGISTER_MODAL');" value="취소" tabindex="3">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_WARNING">
                    <div class="modal-header">
                        <h4 class="modal-title">주의</h4>
                    </div>
                    <div class="modal-body">
                        <p class="iconfont icon-tip">입력값은 유효하지 않습니다.</p>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-primary" onclick="html_f.DisplayNone('MFP_DIALOG_WARNING');html_f.closeModal('MFP_REGISTER_MODAL');" value="닫기">
                    </div>
                </div>
            </div>
        </div>
        <div class="modal" id="MFP_LOGIN_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_LOGIN_LIST">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            MFP 정보 목록
                            <span class="page" id="MFP_LOGIN_PAGE">
                                <a href="javascript:;" class="pre" onclick="html_f.refreshCurrentPage('MFP_LOGIN_PAGE_PRE','MFP_LOGIN_MODAL');" id="MFP_LOGIN_PAGE_PRE">&lt;</a>
                                <span>
                                    <label title="current" id="MFP_LOGIN_PAGE_CUR">1</label>
                                    /
										<label title="total" id="MFP_LOGIN_PAGE_TOTAL">5</label>
                                </span>
                                <a href="javascript:;" class="next" onclick="html_f.refreshCurrentPage('MFP_LOGIN_PAGE_NEXT','MFP_LOGIN_MODAL');" id="MFP_LOGIN_PAGE_NEXT">&gt;</a>
                            </span>
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner">
                            <div class="login-list">
                                <div class="text-formerror table-err">
                                    <div id="InputDataStr_5" class="left-err" style="display:none">입력하지 않았습니다.</div>
                                    <div id="InputDataStr_6" class="left-err" style="display:none">입력 가능 문자수 초과</div>
                                    <div id="InputDataStr_7" class="right-err" style="display:none">입력하지 않았습니다.</div>
                                    <div id="InputDataStr_8" class="right-err" style="display:none">입력 가능 문자수 초과</div>
                                    <div id="InputDataStr_12" class="left-err" style="display:none">무효 글자 포함.</div>
                                </div>
                                <table border="0" cellspacing="0" class="table-interval" id="loginInfor">
                                    <thead>
                                        <tr>
                                            <th width="12%">등록 번호</th>
                                            <th width="30%">IP 주소</th>
                                            <th width="32%">기종명</th>
                                            <th width="26%">옵션</th>
                                        </tr>
                                    </thead>
                                    <tbody></tbody>
                                </table>
                                <div id="MFP_LOGIN_INFOR_OPTION_1" style="display:none;">
                                    <a href="javascript:;" onclick="html_f.loginInforEdit(this);">편집</a>
                                    <a href="javascript:;" onclick="html_f.loginInforDelete(this);">삭제</a>
                                </div>
                                <div id="MFP_LOGIN_INFOR_OPTION_2" style="display:none;">
                                    <a href="javascript:;" title="exsit" onclick="html_f.loginInforExsit(this);">저장</a>
                                    <a href="javascript:;" title="cancel" onclick="html_f.loginInforCancel(this);">취소</a>
                                </div>
                                <div id="MFP_DIALOG_SURE_DELETE" style="display:none;">
                                    <div class="to-sure-delete">
                                        <p>삭제하시겠습니까?</p>
                                        <input type="button" class="btn delete btn-primary" onclick="html_f.sureToDelete(this)" value="삭제">
                                        <input type="button" class="btn btn-default cancel" onclick="html_f.removeSureDelete();" value="취소">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="button" id="localWindowOpen" class="btn btn-default" onclick="html_f.sureLocalOpen('MFP_LOGIN_LIST','MFP_DIALOG_SURE');" value="이 화면에 표시">
                        <input type="button" class="btn btn-primary" id="newWindowOpen" onclick="html_f.newWindowOpen('MFP_LOGIN_MODAL');" value="다른 탭에 표시">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_LOGIN_MODAL');" value="닫기">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_SURE" style="display:none;">
                    <div class="modal-header"></div>
                    <div class="modal-body">
                        <h4 style="font-weight:normal;">이 화면에 표시하는 경우, 지금의 사용자나 관리자를 로그 아웃합니다. 계속하시겠습니까?</h4>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" id="backToSure" onclick="html_f.DisplayBlock('MFP_LOGIN_LIST');html_f.DisplayNone('MFP_DIALOG_SURE');" value="이전">
                        <input type="button" class="btn btn-primary" onclick="html_f.closeModal('MFP_LOGIN_MODAL');html_f.localOpen('MFP_LOGIN_MODAL');" value="이 화면에 표시">
                    </div>
                </div>
            </div>
        </div>
        <div class="modal" id="MFP_SEARCH_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_SEARCH_LIST" style="display:none">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            주변 MFP 검색
                            <span class="page" id="MFP_SEARCH_PAGE">
                                <a href="javascript:;" class="pre" onclick="html_f.refreshCurrentPage('MFP_SEARCH_PAGE_PRE','MFP_SEARCH_MODAL');" id="MFP_SEARCH_PAGE_PRE">&lt;</a>
                                <span>
                                    <label title="current" id="MFP_SEARCH_PAGE_CUR">1</label>
                                    /
										<label title="total" id="MFP_SEARCH_PAGE_TOTAL">5</label>
                                </span>
                                <a href="javascript:;" class="next" onclick="html_f.refreshCurrentPage('MFP_SEARCH_PAGE_NEXT','MFP_SEARCH_MODAL');" id="MFP_SEARCH_PAGE_NEXT">&gt;</a>
                            </span>
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner">
                            <div class="search-list">
                                <table border="0" cellspacing="0" class="table-interval table-has-radio" id="searchList">
                                    <thead>
                                        <tr>
                                            <th width="10%"></th>
                                            <th width="30%">기종명</th>
                                            <th width="40%">IP 주소</th>
                                            <th width="20%">옵션</th>
                                        </tr>
                                    </thead>
                                    <tbody></tbody>
                                </table>
                                <div id="MFP_SEARCH_LIST_OPTION">
                                    <a href="javascript:;" title="MFP 정보 등록" onclick="html_f.getIP(this);html_f.closeModal('MFP_SEARCH_MODAL');html_f.showModal('','MFP_REGISTER_MODAL');html_f.passByValue();html_f.DisplayNone('MFP_OVER_BTN');" class="icon_add"></a>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="button" id="MFP_SEARCH_RESULT_OPEN" class="btn btn-default" onclick="html_f.sureLocalOpen('MFP_SEARCH_LIST','MFP_DIALOG_SURE2');" value="이 화면에 표시">
                        <input type="button" id="MFP_NEW_RESULT_OPEN" class="btn btn-primary" onclick="html_f.newWindowOpen('MFP_SEARCH_MODAL');" value="다른 탭에 표시">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="닫기">
                    </div>
                </div>
                <div class="modal-content" id="MFP_LOADING">
                    <div class="modal-header">
                        <h4 class="modal-title">주변 MFP 검색</h4>
                    </div>
                    <div class="modal-body modal-body-loading">
                        <img width="32" height="32" src="load_wait.gif" alt="">MFP 정보 검색 중...
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="취소">
                    </div>
                </div>
                <div class="modal-content" id="MFP_SEARCH_FAIL" style="display:none;">
                    <div class="modal-header">
                        <h4 class="modal-title">주변 MFP 검색</h4>
                    </div>
                    <div class="modal-body">주변 MFP 검색에 실패하였습니다.</div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-primary" onclick="html_f.DisplayBlock('MFP_LOADING');html_f.DisplayNone('MFP_SEARCH_FAIL');html_f.resetParam();html_f.getMfpList();" value="재검색">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="취소">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_SURE2" style="display:none;">
                    <div class="modal-header"></div>
                    <div class="modal-body">
                        <h4 style="font-weight:normal;">이 화면에 표시하는 경우, 지금의 사용자나 관리자를 로그 아웃합니다. 계속하시겠습니까?</h4>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" onclick="html_f.DisplayBlock('MFP_SEARCH_LIST');html_f.DisplayNone('MFP_DIALOG_SURE2');" value="이전">
                        <input type="button" class="btn btn-primary" onclick="html_f.closeModal('MFP_SEARCH_MODAL');html_f.localOpen('MFP_SEARCH_MODAL');" value="이 화면에 표시">
                    </div>
                </div>
            </div>
        </div>
        <div id="MFP_LOGIN_LIST_FAIL" class="dialog-fail-alert"></div>
        <div id="MFP_COMMON_FAIL_MESSAGE" style="display:none;">시스템 오류가 발생하였습니다.</div>
        <div id="MFP_NO_RESULT" style="display:none;">검색 결과가 없습니다.</div>
        <div id="MainLayout" class="main-layout">
            <div id="mainMenu" class="main-menu-frame">
                <div id="Tab" class="tab-layout" style="display:none">
                    <div class="main-menu">
                        <ul class="main-menu-list-us">
                            <li class="tab tab-system-unselect" id="SystemLayout">
                                <a class="tab tab-unselect" href="system_device.xml" id="SystemFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="SystemIconImg" src="tab_system.png" alt="정보 표시" border="0" hspace="1px">
                                    <span>정보 표시</span>
                                </a>
                            </li>
                            <li class="tab tab-job-unselect" id="JobLayout">
                                <a class="tab tab-unselect" href="job_active.xml" id="JobFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="JobIconImg" src="tab_job.png" alt="작업" border="0" hspace="1px">
                                    <span>작업</span>
                                </a>
                            </li>
                            <li class="tab tab-box-unselect" id="BoxLayout">
                                <a class="tab tab-unselect" href="box_login.xml" id="FileFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="FileIconImg" src="tab_box.png" alt="박스" border="0" hspace="1px">
                                    <span>박스</span>
                                </a>
                            </li>
                            <li class="tab tab-print-unselect" id="PrintLayout">
                                <a class="tab tab-unselect" href="print.xml" id="PrintFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="PrintIconImg" src="tab_print.png" alt="다이렉트 인쇄" border="0" hspace="1px">
                                    <span>다이렉트 인쇄</span>
                                </a>
                            </li>
                            <li class="tab tab-abbr-unselect" id="AbbrLayout">
                                <a class="tab tab-unselect" href="abbr.xml" id="ScanFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="ScanIconImg" src="tab_abbr.png" alt="수신지 등록" border="0" hspace="1px">
                                    <span>수신지 등록</span>
                                </a>
                            </li>
                            <li class="tab tab-fav-unselect" id="FavLayout">
                                <a class="tab tab-unselect" href="favorite.xml" id="FavFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                                    <img id="FavIconImg" src="tab_favorite.png" alt="즐겨찾기 설정" border="0" hspace="1px">
                                    <span>즐겨찾기 설정</span>
                                </a>
                            </li>
                            <li class="tab-top-custom tab-custom-unselect" id="CustomLayout">
                                <a class="tab tab-unselect" href="custom.xml" id="CustomFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="CustomIconImg" src="tab_custom.png" alt="사용자 설정" border="0" hspace="1px">
                                    <span>사용자 설정</span>
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                <div id="Tab-Border" style="display:none"></div>
            </div>
            <div id="subMenu" class="sub-menu-frame">
                <div id="SS3" class="menu-layout" style="display:none">
                    <a href="#ContentsStartLink">
                        <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차로 링크">
                    </a>
                    <div class="menu-box">
                        <div class="menu-main" id="M_BoxLogin">
                            <a id="BoxLogin" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_UOU');html_f.menuLocation('box_login.xml');" style="display:block">
                                <div class="menu-text">박스 열기</div>
                            </a>
                            <button id="BoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_UOU');"></button>
                        </div>
                        <div class="menu-main" id="M_BoxList">
                            <a id="BoxList" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_ULU');html_f.menuLocation('box_list.xml');" style="display:block">
                                <div class="menu-text">박스 목록</div>
                            </a>
                            <button id="BoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_ULU');"></button>
                        </div>
                        <div class="menu-main" id="M_SysBoxLogin">
                            <a id="SysBoxLogin" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_SOU');html_f.menuLocation('box_slogin.xml');" style="display:block">
                                <div class="menu-text">시스템 박스 열기</div>
                            </a>
                            <button id="SysBoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SOU');"></button>
                        </div>
                        <div class="menu-main" id="M_SysBoxList">
                            <a id="SysBoxList" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_SLU');html_f.menuLocation('box_slist.xml');" style="display:block">
                                <div class="menu-text">시스템 박스 목록</div>
                            </a>
                            <button id="SysBoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SLU');"></button>
                        </div>
                    </div>
                    <a name="ContentsStartLink" href="#ContentsStartLink">
                        <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차">
                    </a>
                </div>
            </div>
            <div id="Main" class="body-layout" style="display:none">
                <a href="#PageEndLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="페이지 종료로 링크">
                </a>
                <div class="contents-box">
                    <input type="hidden" name="HddInst" id="HddInst" value="true">
                    <div id="F_ULU" class="contents-layout" style="display:block;">
                        <div id="MC_BoxList" class="widget-separator">
                            <input type="hidden" id="HELP_F_ULU" value="?Menu=UBox_A@Contents=menuUserDE">
                            <div id="BoxHtml" style="display:block;">
                                <input type="hidden" id="F_ULUUsertrueH_BDP" value="true">
                                <input type="hidden" name="H_TAB" id="F_OPN_TAB" value="">
                                <input type="hidden" id="F_ULU_AUN" value="Public">
                                <h1 class="contents-header">
                                    박스 목록<img src="arrow.png" id="WS_MC_BoxList" alt=" " class="move-contents" onclick="moveToFavorite('MC_BoxList');" onmouseover="ContentsDragDropRegist('MC_BoxList');" onmouseout="ContentsDragDropDestroy('MC_BoxList');">
                                </h1>
                                <div class="contents-body">
                                    <label id="TopLbl">신규 사용자 박스를 등록할 수 있습니다.</label>
                                    <table class="data-table-noline" width="500px">
                                        <tr>
                                            <td width="100px">
                                                <input type="button" value="새 등록" id="F_ULU_Bregist" class="btn btn-auto" onclick="javascript:html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_ULU');box_f.MoveUrl('box_create.xml');">
                                            </td>
                                            <td width="400px"></td>
                                        </tr>
                                    </table>
                                </div>
                                <form method="post" id="F_ULUtrue_PA" name="F_ULU_PAG" action="user.cgi" style="margin: 0px;" Accept-charset="UTF-8">
                                    <input type="hidden" name="H_TAB" id="H_F_BOXLIST_PA" value="">
                                    <input type="hidden" id="H_FN" name="func" value="PSL_F_ULU_PAG">
                                    <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                                    <input type="hidden" id="H_SE" name="H_SER" value="">
                                    <table class="data-table-noline box-data-table-noline" width="505px">
                                        <tr>
                                            <td width="180px" class="td-indent7"></td>
                                            <th scope="row" width="150px" valign="top">페이지(50개 표시)</th>
                                            <td width="80px">
                                                <select id="S_PA" class="select-page" name="S_PAG">
                                                    <option value="1" selected>1</option>
                                                </select>
                                            </td>
                                            <td width="46px"></td>
                                        </tr>
                                    </table>
                                </form>
                                <form method="post" id="F_ULUtrue_SE" action="user.cgi" style="margin: 0px;" Accept-charset="UTF-8">
                                    <input type="hidden" name="H_TAB" id="H_F_BOXLIST_SE" value="">
                                    <table class="data-table-noline box-data-table-noline" width="505px">
                                        <tr>
                                            <td width="180px" class="td-indent7"></td>
                                            <th scope="row" width="150px" valign="top">검색문자로 검색</th>
                                            <td width="80px">
                                                <input type="hidden" name="func" value="PSL_F_ULU_PAG">
                                                <input type="hidden" id="H_PA" name="H_PAG" value="">
                                                <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                                                <select id="S_SE" class="select-label" name="S_SER">
                                                    <option value="All">모두</option>
                                                    <option value="Abc">ABC</option>
                                                    <option value="Def">DEF</option>
                                                    <option value="Ghi">GHI</option>
                                                    <option value="Jkl">JKL</option>
                                                    <option value="Mno">MNO</option>
                                                    <option value="Pqrs">PQRS</option>
                                                    <option value="Tuv">TUV</option>
                                                    <option value="Wxyz">WXYZ</option>
                                                    <option value="Other">기타</option>
                                                </select>
                                            </td>
                                            <td width="35px">
                                                <input type="button" id="S_PA_GO" class="btn btn-small" value="Go" onclick="box_f.setSearch(document.getElementById('S_PA').options[document.getElementById('S_PA').selectedIndex].value,'F_ULUtrue_PA');">
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                                <br>
                                <form id="User_LI" Accept-charset="UTF-8">
                                    <table class="data-table data-box-table" width="500px" style="border-layout: fixed;">
                                        <tr>
                                            <th scope="col">박스 번호</th>
                                            <th scope="col">박스 이름</th>
                                            <th scope="col">박스 종류</th>
                                            <th scope="col">소유자 이름</th>
                                            <th scope="col" colspan="2">박스 조작</th>
                                        </tr>
                                        <tr>
                                            <td width="75px">
                                                1<br>
                                            </td>
                                            <td width="110px">
                                                <label style="margin-left:20px;">하우디6</label>
                                            </td>
                                            <td width="100px">
                                                공유<br>
                                            </td>
                                            <td width="110px">공유</td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="hidden" id="1_PAF_NotRelayAuthOn" value="false">
                                                    <input type="hidden" id="1_BID" value="1">
                                                    <input type="hidden" id="1_BNM" value="하우디6">
                                                    <input type="button" value="편집" id="F_ULU_Bset" name="F_ULU_Bset" class="btn btn-style-form" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','1','true','_NotRelayAuthOn', 'Setting');">
                                                </div>
                                            </td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="button" value="삭제" id="F_ULU_Bdel" name="F_ULU_Bdel" class="btn btn-style-form" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','1','true','_NotRelayAuthOn', 'Delete');">
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="75px">
                                                2<br>
                                            </td>
                                            <td width="110px">
                                                <label style="margin-left:20px;">test</label>
                                            </td>
                                            <td width="100px">
                                                공유<br>
                                            </td>
                                            <td width="110px">공유</td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="hidden" id="2_PAF_NotRelayAuthOn" value="false">
                                                    <input type="hidden" id="2_BID" value="2">
                                                    <input type="hidden" id="2_BNM" value="test">
                                                    <input type="button" value="편집" id="F_ULU_Bset" name="F_ULU_Bset" class="btn btn-style-form" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','2','true','_NotRelayAuthOn', 'Setting');">
                                                </div>
                                            </td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="button" value="삭제" id="F_ULU_Bdel" name="F_ULU_Bdel" class="btn btn-style-form" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','2','true','_NotRelayAuthOn', 'Delete');">
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                                <form method="POST" name="F_ULU_PTR" id="F_ULUtrueUser_PTR" onsubmit="return box_f.ChkList('F_ULUtrueUser_');" action="user.cgi" Accept-charset="UTF-8">
                                    <input type="hidden" name="H_TAB" id="H_F_BOXLIST_PTR" value="">
                                    <input type="hidden" name="func" value="PSL_F_ULUUser_BOX">
                                    <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                                    <input type="hidden" name="H_BID" id="F_ULUtrueUser_BID" value="">
                                    <input type="hidden" name="H_DSP" value="">
                                    <input type="hidden" name="H_IPA" value="On">
                                    <input type="hidden" name="H_PID" value="-1">
                                    <input type="hidden" name="T_BID" value="">
                                </form>
                                <input type="hidden" id="errLang" value="lang_err_Ko.xml">
                                <input type="hidden" id="Box_Load" onclick="box_f.callOnLoad();">
                                <div id="F_ULUDialogContent" style="display:none;" class="dialog-content">
                                    <div class="box-psd-inpute" id="F_ULUBoxPsdInput">
                                        <div id="commoncontent">
                                            <div class="common-div-msg">
                                                <div class="common-topbar-msg">사용자 박스 암호를 입력하십시오.</div>
                                                <div id="F_ULUboxNo" class="list-box-no">
                                                    <label id="lblBoxNo">박스 번호</label>
                                                    <label id="F_ULU_BID"></label>
                                                </div>
                                                <div id="F_ULUboxPassword" class="box-password">
                                                    <label id="lblPassword">암호</label>
                                                    <input id="F_ULU_BPA" type="password">
                                                </div>
                                                <div class="login-bottom">
                                                    <input id="F_ULU_Apply" type="button" value="확인" class="btn btn-big ok">
                                                    <input id="F_ULU_Cancel" type="button" value="취소" class="btn btn-big cal" onclick="box_f.hidePsdDialog('F_ULU');">
                                                </div>
                                                <span id="F_ULUPsdInputErr" class="psd-input-err"></span>
                                            </div>
                                        </div>
                                    </div>
                                    <div style="display:none">
                                        <span id="lang_type_BulletinBoard">게시판 박스</span>
                                        <span id="lang_type_Confidential">친전 수신 박스</span>
                                        <span id="lang_type_Relay">중계 박스</span>
                                        <span id="lang_type_FilingNumber">파일링 넘버 박스</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="F_ULUUserBoxLogin" class="contents-layout" style="display:none;">
                        <form method="POST" name="F_ULU_PTR" id="F_ULU_PT" action="user.cgi" onsubmit="box_f.setObjBoxNo('F_ULUUserfalse_HID');return box_f.ChkList('F_ULUUserfalse_','false','User');" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_ULUUser_BOX">
                            <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_BID" id="F_ULUUserfalse_HID">
                            <input type="hidden" id="F_ULUUserfalseH_BDP" value="false">
                            <input type="hidden" name="H_DSP" value="">
                            <input type="hidden" name="H_TAB" id="H_F_F_ULU_PT" value="">
                            <h1 class="contents-header">박스 열기(공유)</h1>
                            <table class="text-helpmessage">
                                <tr>
                                    <td>
                                        박스는 본체내에 문서를 저장할 수 있는 기능입니다.<br>
                                        박스내의 문서는 인쇄/전송 등에 사용할 수 있습니다.<br>
                                    </td>
                                </tr>
                            </table>
                            <div class="contents-body">
                                <table class="data-table-noline" width="97%">
                                    <tr>
                                        <td class="td-indent1" scope="row" width="240px">박스 암호</td>
                                        <td width="260px">
                                            <input type="password" name="P_BPA" id="F_ULUUserfalse_BPA" size="10" maxlength="64">
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <hr class="contents-end" title="">
                            <div class="buttonarea-layout">
                                <input type="button" value="확인" id="F_ULUUserfalse_Apply" onclick="box_f.setCookie('F_ULU');box_f.FormSubmit('F_ULU_PT')" class="button-height">
                                <input type="reset" id="F_ULUUserfalse_Clear" value="지우기" onclick="box_f.ClearErr();box_f.repairBoxIdDelay('F_ULU','User');" style="display:none">
                                <input type="button" value="취소" id="F_ULUUserfalse_Cancel" onclick="box_f.setFi('F_ULU','F_ULU');box_f.formReset('F_ULUNewForm');" class="button-height">
                            </div>
                        </form>
                    </div>
                </div>
                <a name="PageEndLink" href="#PageEndLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="페이지 종료">
                </a>
            </div>
            <div class="contents-box">
                <div class="body-layout" style="display:block;">
                    <div id="LowBatt" class="contents-layout" style="display:none;">
                        <input type="hidden" id="HELP_LOWBATT" value="?Menu=top@Contents=LowBatt">
                        <form id="AS_LB" name="AS_LOW" method="POST" action="user.cgi" onsubmit="html_f.setCgiURL('H_URL_LOWBATT');html_f.btnDisabled();html_f.contentsButtonDisabled(id);" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_D_PSV">
                            <input type="hidden" name="H_JOB" id="H_JOB_LOWBATT" value="">
                            <input type="hidden" name="H_PLS" id="H_PLS_LOWBATT" value="">
                            <input type="hidden" name="H_URL" id="H_URL_LOWBATT" value="">
                            <h1 class="contents-header">절전 모드</h1>
                            <P>
                            <div class="contents-body">
                                <div id="NoLowBattOnJobOnAdmin" style="display:none;">절전모드로 이동할 수 없습니다. 장치가 관리자 모드이거나, 진행중인 작업이 있습니다.</div>
                                <div id="NoLowBattOnJob" style="display:none;">절전모드로 이동할 수 없습니다. 진행중인 작업이 있습니다.</div>
                                <div id="NoLowBattOnAdmin" style="display:none;">절전모드로 이동할 수 없습니다. 장치가 관리자 모드이거나, 작업이 진행중입니다.</div>
                                <div id="ConfLowBattOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중입니다. 절전 모드로 이동하시겠습니까?</div>
                                <div id="ConfLowBatt" style="display:none;">절전 모드로 이동하시겠습니까?</div>
                            </div>
</P><hr class="contents-end" title="종료">
<div class="buttonarea-layout">
    <div id="LowBattOK" style="display:none;">
        <input type="submit" value="확인" class="button-height">
        <input type="button" value="취소" onclick="html_f.ClearLowBattery()" class="button-height">
    </div>
    <div id="LowBattNG" style="display:none;">
        <input type="button" value="확인" onclick="html_f.ClearLowBattery()">
    </div>
</div>
</form></div>
<div id="Logout" class="contents-layout" style="display:none;">
    <input type="hidden" id="HELP_LOGOUT" value="?Menu=top@Contents=menuLoginB">
    <form id="AS_LO" name="AS_LGO" method="POST" action="user.cgi" onsubmit="html_f.closeDialogAll();html_f.btnDisabled();html_f.contentsButtonDisabled(id);html_f.resetAbbrCheckCookie();" Accept-charset="UTF-8">
        <input type="hidden" name="func" value="PSL_ACO_LGO">
        <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
        <h1 class="contents-header">로그아웃</h1>
        <div class="contents-body">
            <p>공유 사용자로부터 로그아웃 하고, 로그인 화면으로 이동하시겠습니까?</p>
        </div>
        <hr class="contents-end" title="종료">
        <div class="buttonarea-layout">
            <input type="submit" value="확인" class="button-height btn-style-bottom btn-style-ok">
            <input type="button" value="취소" onclick="html_f.ComReturnDisplay()" class="button-height btn-style-bottom">
        </div>
        <div class="bottom-area"></div>
    </form>
</div>
<div id="Reboot" class="contents-layout" style="display:none;">
    <input type="hidden" id="HELP_REBOOT" value="?Menu=top@Contents=Reboot">
    <form id="AS_RB" name="AS_REB" method="POST" action="a_user.cgi" onsubmit="html_f.setCgiURL('H_URL_REBOOT');html_f.btnDisabled();html_f.contentsButtonDisabled(id);" Accept-charset="UTF-8">
        <input type="hidden" name="func" value="PSL_D_REB">
        <input type="hidden" name="H_JOB" id="H_JOB_REBOOT" value="">
        <input type="hidden" name="H_PLS" id="H_PLS_REBOOT" value="">
        <input type="hidden" name="H_URL" id="H_URL_REBOOT" value="">
        <h1 class="contents-header">재시작</h1>
        <P>
        <div class="contents-body">
            <div id="ConfRebootOnJobOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중이고, 실행중인 작업이 있습니다.재기동을 하시겠습니까?</div>
            <div id="ConfRebootOnJob" style="display:none;">실행중인 작업이 있습니다. 재기동을 하시겠습니까?</div>
            <div id="ConfRebootOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중입니다. 재기동을 하시겠습니까?</div>
            <div id="ConfReboot" style="display:none;">재기동을 하시겠습니까?</div>
        </div>
</P><hr class="contents-end" title="종료">
<div class="buttonarea-layout">
    <input type="submit" value="확인" class="button-height">
    <input type="button" value="취소" onclick="html_f.ClearReboot()" class="button-height">
</div>
</form></div></div></div></div><iframe src="box_f.html" name="box_f" width="0" height="0" frameborder="0"></iframe>
<iframe src="box_f.html" name="box_system_f" width="0" height="0" frameborder="0"></iframe>
<iframe src="box_f.html" name="html_f" width="0" height="0" frameborder="0"></iframe>
<iframe name="livecheck" id="livecheck" src="livecheck.html" frameborder="0" height="0" width="0"></iframe>
</body><script type="text/javascript" src="prototype.js"></script>
<script type="text/javascript" src="effects.js"></script>
<script type="text/javascript" src="dragdrop.js"></script>
<script type="text/javascript" src="menutable.js"></script>
<script type="text/javascript" src="movectrl.js"></script>
</html>


## 박스 생성
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
Status Code
200 OK
페이로드:
func=PSL_F_ULUUser_CRE&h_token=cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy&H_TAB=&R_NUM=Space&T_NAM=TEST_NAME&C_USE=UsePass&P_PAS=1234&P_PAS2=1234&S_SER=Abc&R_SAP=None&S_GFC=On&S_SEC=Off
* T_NAME: 박스 이름, C_USE: 패스워드 사용 여부, P_PAS: 암호, 그 외 설정 고정

## 박스 수정
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
Status Code
200 OK
페이로드: H_TAB=&func=PSL_F_ULU_SET&h_token=cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy&H_BOX=3&H_USR=&H_BPA=&H_BTY=User&H_BAT=Public&H_XTP=&H_DSP=Setting&T_NAM=TEST_NAME1&S_SER=Abc&R_SAP=None&C_PAC=on&P_CPA=1234&P_NPA=123456&P_NPA2=123456
* P_CPA: 기존 비밀번호, P_NPA: 신규 비밀번호

## 박스 삭제
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
페이로드: func=PSL_F_ULU_DEL&h_token=cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy&H_BOX=3&H_USR=&H_BPA=&H_NAM=TEST_NAME1&H_SAV=0&H_BTY=User&H_XTP=&H_DCNT=0&H_TAB=

## 박스 내 파일 목록 조회
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
Status Code
200 OK
페이로드: func=PSL_F_UOUUser_BOX&H_BID=3&T_BID=3&P_BPA=1234&h_token=cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy&H_PID=-1&H_IPA=On&_=

응답:
<?xml version="1.0" encoding="UTF-8"?><?xml-stylesheet href="./waitmove.xsl" type="text/xsl"?><MFP><TemplateName>waitmove.xsl</TemplateName><Function>err</Function><LangNo>Ko</LangNo><RedirectUrl>box_detail.xml</RedirectUrl><Interval>100</Interval><MyTab>false</MyTab><NoMessage>true</NoMessage><Message></Message><CgiAction>void</CgiAction><CgiIdentity>void</CgiIdentity><CancelDisp>false</CancelDisp></MFP>

Request URL
http://192.168.11.241/wcd/waitmsg
Request Method
POST
Status Code
200 OK
페이로드: TaskNo=0&_=
응답: <?xml version="1.0" encoding="UTF-8"?><?xml-stylesheet href="./wait.xsl" type="text/xsl"?><MFP><TemplateName>wait.xsl</TemplateName><Function>err</Function><LangNo>Ko</LangNo><RedirectUrl>waitmsg</RedirectUrl><Interval>2000</Interval><MyTab>false</MyTab><NoMessage>false</NoMessage><Message><Item Code="DeviceExportExec"/></Message><CgiAction>waitmsg</CgiAction><CgiIdentity>void</CgiIdentity><CancelDisp>false</CancelDisp><ParamName>TaskNo</ParamName><ParamValue>0</ParamValue></MFP>

Request URL
http://192.168.11.241/wcd/box_detail.xml
Request Method
POST
Status Code
200 OK
페이로드: waitend=true&TaskNo=0&_=
응답: <?xml version="1.0" encoding="UTF-8"?>
<?xml-stylesheet href="box_detail.xsl" type="text/xsl"?>
<MFP>
<DisplayMode></DisplayMode>
<Token>cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy</Token><Common>
<SelNo>Auto</SelNo>
<LangNo>Ko</LangNo>
<Favorite>On</Favorite>
<AuthUserName>Public</AuthUserName>
<LoginServerIndex>0</LoginServerIndex><LoginMode>PublicUser</LoginMode><LoginName></LoginName><EnableWebConnection>true</EnableWebConnection><PcmSsId>30</PcmSsId>
<Simulator>false</Simulator>
<CertInstalled>false</CertInstalled>
<GreekEnable>true</GreekEnable><HebrewEnable>true</HebrewEnable><EncryptEnable>true</EncryptEnable><JpegEnable>false</JpegEnable><KeyVndBoxEnable>Off</KeyVndBoxEnable><BoxSort>false</BoxSort><DN30B1>Off</DN30B1><DN27B8>Off</DN27B8><DN38B8>Off</DN38B8><DN74B7>Off</DN74B7><DN35B3>Off</DN35B3><DN35B4>Off</DN35B4><DN41B2>Off</DN41B2><DN39B1>Off</DN39B1><DN42B8>Off</DN42B8><DN61B3>Off</DN61B3>
<DN75B3>Off</DN75B3><DN47B7>Off</DN47B7><DN49B7>Off</DN49B7><DN70B6>Off</DN70B6><DN52B5>Off</DN52B5><DN64B2>Off</DN64B2><DN65B7>Off</DN65B7><DN65B2>Off</DN65B2>
<DN69B2>Off</DN69B2>
<DN69B6>Off</DN69B6>
<DN80B4>On</DN80B4>
<DN71B4>Off</DN71B4>
<DN71B7>Off</DN71B7>
<DN92B7>Off</DN92B7>
<DN72B3>Off</DN72B3>
<DN72B5>Off</DN72B5>
<DN71B4>Off</DN71B4>
<DN73B1>Off</DN73B1>
<DN75B5>Off</DN75B5>
<DN75B8>Off</DN75B8>
<DN77B6>Off</DN77B6>
<DN75B1>Off</DN75B1>
<DN215B2>Off</DN215B2>
<PswcDirectPrint>On</PswcDirectPrint><DN79B6>Off</DN79B6>
<DN79B5>Off</DN79B5><DN130B1>Off</DN130B1><DN84B8>Off</DN84B8><DN4B1>Off</DN4B1><DN89B8>Off</DN89B8><DN90B1>Off</DN90B1><DN90B2>Off</DN90B2><DN90B3>On</DN90B3><DN90B4>Off</DN90B4><DN90B5>On</DN90B5><DN91B1>Off</DN91B1><DN91B2>On</DN91B2><DN91B3>Off</DN91B3><DN87B1>Off</DN87B1><DN89B4>Off</DN89B4>
<DN74B8>Off</DN74B8>
<DN76B5>Off</DN76B5><DN82B1>Off</DN82B1><DN101B5>On</DN101B5><DN112B6>Off</DN112B6>
<DN107B1>Off</DN107B1><DN119B3>Off</DN119B3><DN94B2>Off</DN94B2><DN109>2</DN109><IsGeneric>On</IsGeneric>
<DN111B6>Off</DN111B6>
<DN121B4>Off</DN121B4>
<DN122B8>Off</DN122B8>
<MyAddressEnable>Off</MyAddressEnable>
<KeyDeviceList><ArraySize>5</ArraySize><KeyDevice><Type>KeyCounter</Type><Status>False</Status><Installed>False</Installed></KeyDevice><KeyDevice><Type>ExternalManagementDevice</Type><Status>False</Status><Installed>False</Installed></KeyDevice><KeyDevice><Type>Vendor</Type><Status>False</Status><Installed>False</Installed></KeyDevice><KeyDevice><Type>ExternalManagementDeviceKM</Type><Status>False</Status><Installed>False</Installed></KeyDevice><KeyDevice><Type>VendorKM</Type><Status>False</Status><Installed>False</Installed></KeyDevice></KeyDeviceList><Info>
<EngType>OTHER</EngType>
</Info>
<FuncVer>10</FuncVer>
<DisplayErrorCode>Off</DisplayErrorCode><SendingDomainLimit>NoLimit</SendingDomainLimit><IsAral65>True</IsAral65><DisplayID>NONE</DisplayID><AddressLock>false</AddressLock><ColorFlag2>True</ColorFlag2><HDDLessFax>false</HDDLessFax>
<SupportFunction><AppOption><Preview>On</Preview><Stamp>On</Stamp><SendOperatorLog>On</SendOperatorLog><InspectLog>On</InspectLog><FileTypeNoLicence>On</FileTypeNoLicence><FileTypeWithLicence>Off</FileTypeWithLicence><Fax>On</Fax><BizhubRA>On</BizhubRA><SearchLdap>On</SearchLdap><Bmlinks>Off</Bmlinks><UbiquitousStorage>Off</UbiquitousStorage><UbiquitousClient>Off</UbiquitousClient><WebBrowser>On</WebBrowser><Iws>Off</Iws><UserControlCnt>On</UserControlCnt><Overlay>Off</Overlay></AppOption><MfpEquipModel>Performance</MfpEquipModel><FwRollbackApplicable>On</FwRollbackApplicable><LocalInterfaceKit>Off</LocalInterfaceKit><PlainPaperExApplicable>On</PlainPaperExApplicable>
<OOXMLFontData>On</OOXMLFontData>
</SupportFunction><Fiery>false</Fiery><EnableServerGroup>On</EnableServerGroup><EnableSimpleServerGroup>On</EnableSimpleServerGroup><EnableIccServerGroup>Off</EnableIccServerGroup><LScaleEnable>Invalid</LScaleEnable><DataRegistrationExtension>Off</DataRegistrationExtension><SecurityLevel>None</SecurityLevel><BoxCreatePermission>On</BoxCreatePermission>
<EnableBrowserInfoUser>Off</EnableBrowserInfoUser>
<DistributeSetting><Enable>On</Enable></DistributeSetting>
<ScanFunctionEnable>true</ScanFunctionEnable>
<MFPSmbFileSharing>On</MFPSmbFileSharing>
<MFPSmbServer>On</MFPSmbServer>
<PanelFlag>false</PanelFlag><ReqClientAddr>192.168.11.184</ReqClientAddr>
<ReqHostIpAddr>192.168.11.241</ReqHostIpAddr>
<System><ProductName>SINDOH D420</ProductName><ProductID>1.3.6.1.4.1.18334.1.2.1.2.1.131.3.9</ProductID><DeviceID></DeviceID><Oem>true</Oem><SerialNumber>792080652279</SerialNumber><ExternalIfMode>None</ExternalIfMode><ControllerInfoList><ArraySize>1</ArraySize><ControllerInfo><Type>Printer</Type><Name>SINDOH D420</Name><Version>A7PU0Y0-3000-G00-R2</Version></ControllerInfo></ControllerInfoList><SupportFunction><Copy>true</Copy><Print>true</Print><Scan>true</Scan><Fax>true</Fax><Fax2>false</Fax2><Fax3>Off</Fax3><Fax4>Off</Fax4><SipAdapter>false</SipAdapter><Ifax>false</Ifax><IpAddressFax>Off</IpAddressFax><JobNumberDisplayFunc>On</JobNumberDisplayFunc><UsbHostBoard>On</UsbHostBoard><Dsc>Off</Dsc><Dsc2>Off</Dsc2><Bluetooth>Off</Bluetooth><DualScan>Off</DualScan><InternalWebServer>On</InternalWebServer><CustomDocumentMode>Off</CustomDocumentMode><ExpansionNetworkAdapter>Off</ExpansionNetworkAdapter><DsBoard>Off</DsBoard><SlidePanel>Off</SlidePanel><AllowIPFilterSetting>On</AllowIPFilterSetting><BillingCounter>Off</BillingCounter><PowerSaveTimeUpperLimitChange>Off</PowerSaveTimeUpperLimitChange><TxReportImageAttach>On</TxReportImageAttach><NetworkIf>SingleWired</NetworkIf><TouchPanel>Electrostatic</TouchPanel><Sip>false</Sip><IPAddressFax>false</IPAddressFax><FaxBoard>true</FaxBoard><FaxBoard2>false</FaxBoard2><FaxBoard3>false</FaxBoard3><FaxBoard4>false</FaxBoard4></SupportFunction><GeneralContact><SiteName></SiteName><Info></Info><ProductHelpUrl></ProductHelpUrl><CorpUrl></CorpUrl><SupplyInfo></SupplyInfo><PhoneNumber></PhoneNumber><EmailAddress></EmailAddress><UtilityLink></UtilityLink><OnlineHelpUrl></OnlineHelpUrl><DriverUrl></DriverUrl></GeneralContact><UserContact><Contact></Contact><Name></Name><Location>792080652279</Location><InternalNumber></InternalNumber></UserContact><Time><Year>2026</Year><Month>4</Month><Day>24</Day><Hour>14</Hour><Minute>10</Minute><Second>36</Second><TimeZone2><GmtDirection>East</GmtDirection><Hour>9</Hour><Minute>0</Minute></TimeZone2><TimeZone>East_9_00</TimeZone></Time><ExternalIfMode2>None</ExternalIfMode2><FunctionStatus><FunctionCode>0000000000000000000000000000000000000000000000000000010000000000</FunctionCode><WaitReboot>0000000000000000000000000000000000000000000000000000000000000000</WaitReboot><FunctionCodeReverse>0000000000100000000000000000000000000000000000000000000000000000</FunctionCodeReverse></FunctionStatus>
<Oem2>true</Oem2></System><DeviceInfo><Option><Hdd><Installed>true</Installed><Capacity>238454</Capacity></Hdd><Ssd><Installed>exist</Installed></Ssd><Memory><Installed>true</Installed><Capacity>2374</Capacity></Memory><Duplex><Installed>true</Installed><Type>Cycle</Type></Duplex><Adf><Installed>true</Installed><Type>Duplex</Type></Adf><CardAuthenticationDevice>Off</CardAuthenticationDevice>
<BiometricAuthenticationDevice>Off</BiometricAuthenticationDevice>
<LoadableDevice></LoadableDevice>
<EnableCardType></EnableCardType>
<WirelessAdapterType>Notattached</WirelessAdapterType>
</Option></DeviceInfo><DeviceStatus><ScanStatus>210036</ScanStatus>
<PrintStatus>130016</PrintStatus>
<Processing>0</Processing>
<JamCode>:</JamCode>
<NetworkErrorStatus>48</NetworkErrorStatus>
<KmSaasgw>2</KmSaasgw>
<HddMirroringErrorStatus>48</HddMirroringErrorStatus>
<DisplayJamCode>Off</DisplayJamCode></DeviceStatus><Service>
<Info><MarketArea>Europe</MarketArea></Info><Setting>
<AuthSetting><SynchronizedTrack></SynchronizedTrack><AuthMode><ListOn>false</ListOn>
<PublicUser>true</PublicUser>
<BoxAdmin>false</BoxAdmin>
<AuthorityPermission>Off</AuthorityPermission>
<SendAddressLimit>Off</SendAddressLimit>
<AuthType>None</AuthType>
<MiddleServerUse>Off</MiddleServerUse>
<DefaultAuthType></DefaultAuthType>
</AuthMode><TrackMode>
<TrackType>None</TrackType>
</TrackMode><CommonMode>
<NoAuthPrintOn>false</NoAuthPrintOn>
</CommonMode>
<UserAndTrack>
<ColorManage>Color</ColorManage>
</UserAndTrack>
<AuthDataSearch><Enable>Off</Enable></AuthDataSearch>
</AuthSetting><SystemConnection><AdminSend>Off</AdminSend><PrefixSuffix>Off</PrefixSuffix><MobilePrint>On</MobilePrint><ConnectApplication><MyPanelConnect>On</MyPanelConnect><MySpoolConnect>On</MySpoolConnect></ConnectApplication><ChangeUserDataPermission>Off</ChangeUserDataPermission><MobileConnection><QrCodeDisplay>Off</QrCodeDisplay><NfcSetting>Off</NfcSetting><BluetoothLeSetting>Off</BluetoothLeSetting><QrCodeSetting><WirelessConnection><Enable>Off</Enable><ConnectionType>MfpWirelessLanSetting</ConnectionType><IndividualSetting><Ssid></Ssid><AuthEncryptAlgorithm>None</AuthEncryptAlgorithm><WepKey><InputMethod>Ascii64bit</InputMethod><Key></Key></WepKey><PassPhraseString>Ascii</PassPhraseString><PassPhrase></PassPhrase></IndividualSetting></WirelessConnection></QrCodeSetting><AarStartingApp>PageScopeMobile</AarStartingApp></MobileConnection></SystemConnection><General>
<KeyMode>None</KeyMode><IntegrationManagementAuth>Off</IntegrationManagementAuth><Panel><Language>Ko</Language></Panel>
<Security><PasswordAgreement>false</PasswordAgreement><SecurityLevel>None</SecurityLevel><SecurePrintLimited>Off</SecurePrintLimited><CopyGuardEnable>Off</CopyGuardEnable><PasswordCopyEnable>Off</PasswordCopyEnable></Security>
<ForcedPrintInPrinting_Jimon>Off</ForcedPrintInPrinting_Jimon><ForcedPrintInPrinting_CopyGuard>Off</ForcedPrintInPrinting_CopyGuard><ForcedPrintInPrinting_PasswordCopy>Off</ForcedPrintInPrinting_PasswordCopy><ForcedPrintInPrinting_Watermark>Off</ForcedPrintInPrinting_Watermark></General>
</Setting>
</Service>
</Common>
<DeviceInfo>
<ExtensionFunction><MyPanel>Off</MyPanel><MyAddress>Off</MyAddress><CompactXps>On</CompactXps><SearchablePdfDictionary>Off</SearchablePdfDictionary><FontPdf_A>On</FontPdf_A><VoiceData>Off</VoiceData></ExtensionFunction></DeviceInfo>
<DocumentNumberTotal>0</DocumentNumberTotal>
<Job>
<BoxInfoList>
<BoxInfo><BoxID>3</BoxID><BoxAttribute><Category>Functional</Category><Type>User</Type><Attribute>Public</Attribute></BoxAttribute><Name>TEST_SCAN</Name><SearchKey>Abc</SearchKey><Confidential>false</Confidential><CreatorName>Public</CreatorName><CreateTime><Year>2026</Year><Month>4</Month><Day>24</Day><Hour>14</Hour><Minute>6</Minute><Second>26</Second></CreateTime><NumberOfFile>1</NumberOfFile><LifeTime>0</LifeTime><LifeTimeMinute>0</LifeTimeMinute><ValidPassword>true</ValidPassword><GenFormatAutoCnv>On</GenFormatAutoCnv><SmbEncryption>Off</SmbEncryption></BoxInfo><BoxJobInfoList><BoxJobInfo>
<BoxJobID>1</BoxJobID><JobName>S25C-926042414080</JobName><JobTime><CreateTime><Year>2026</Year><Month>04</Month><Day>24</Day><Hour>14</Hour><Minute>08</Minute></CreateTime></JobTime><KindOfJob><JobType>Send</JobType></KindOfJob><FileType>CompactPdf</FileType><FileTypeBoxSend>CompactPdf</FileTypeBoxSend><CPdfConversion>On</CPdfConversion><JobAllowedOperationInfoList><ArraySize>6</ArraySize><AllowedFunction>Print</AllowedFunction><AllowedFunction>SendEmail</AllowedFunction><AllowedFunction>SendSMB</AllowedFunction><AllowedFunction>SendFTP</AllowedFunction><AllowedFunction>SendFax</AllowedFunction><AllowedFunction>Get</AllowedFunction></JobAllowedOperationInfoList><RestrictChangeOutputColor>Off</RestrictChangeOutputColor><RestrictChangeOutputSize>Off</RestrictChangeOutputSize><CopyProtectDetect>0</CopyProtectDetect><RxMode>Non</RxMode><ColorType>FullColor</ColorType><DocumentNumber>1</DocumentNumber><FileType>CompactPdf</FileType><EncipherPdfSetting><EncipherLevel>None</EncipherLevel></EncipherPdfSetting><Resolution><X>300</X><Y>300</Y></Resolution><PageShoot>Off</PageShoot></BoxJobInfo></BoxJobInfoList>
</BoxInfoList>
<BoxCreatePermission>On</BoxCreatePermission>
</Job>
<AllDocumentList>@1@</AllDocumentList><PrintDocumentList>@1@</PrintDocumentList><SnedDocumentList>@1@</SnedDocumentList><DownloadDocumentList>@1@</DownloadDocumentList><FaxSendDocumentList>@1@</FaxSendDocumentList><FullColorDocumentList>@1@</FullColorDocumentList><Service>
<Setting>
<Security>
<DeleteBoxDocument><Enable>On</Enable><LifeTime>Day1</LifeTime></DeleteBoxDocument>
</Security>
<WindowsNetwork><ServerSetting><ServerCommon><SmbServer>On</SmbServer></ServerCommon><FileSharingSetting><SmbFileSharing>On</SmbFileSharing></FileSharingSetting></ServerSetting></WindowsNetwork>
<BoxConfiguration><GenFormatAutoCnvBoxConfiguration><NumberOfBox>2</NumberOfBox><MaxNumberOfBox>300</MaxNumberOfBox></GenFormatAutoCnvBoxConfiguration></BoxConfiguration>
<Fax><ReceiveDataProtectSetting><DataProtectType>NotProtect</DataProtectType><DeletePasswordExist>false</DeletePasswordExist></ReceiveDataProtectSetting></Fax>
</Setting>
</Service>
<BoxExport>false</BoxExport>
<OpeSelect>AllDocument</OpeSelect><DocSelect></DocSelect><CfdSelect></CfdSelect><DispType>Thumbnail</DispType><CurrentPage>-1</CurrentPage><DocStructure><Page>1</Page></DocStructure></MFP>

## 파일 선택
Request URL
http://192.168.11.241/wcd/box_jobdetail.xml
Request Method
GET
Status Code
200 OK

응답:
<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=yes">
        <meta content="text/javascript" http-equiv="Content-Script-Type">
        <script type="text/javascript" charset="utf-8">
            function getFlashVars_Status() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User&ExternalIfMode=None"
            }
            function getFlashVars_InputTray() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User"
            }
            function getFlashVars_ActiveJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User&Favorite=" + Favorite + ""
            }
            function getFlashVars_DoneJob() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User"
            }
            function getFlashVars_Commlist() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User"
            }
            function getFlashVars_AccumulationJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=32&UserMode=User&Favorite=" + Favorite + ""
            }
            function setLocation(url) {
                job_f.btnDisabled();
                location.href = url;
            }
            function setFavoriteLocation(url) {
                parent.MyTab.PortalMain.prototype.getAjaxProc(url);
            }
            function callSetProcWidget() {
                parent.MyTab.PortalMain.prototype.setProcWidget();
            }
            function callHideWait() {
                parent.MyTab.PortalMain.prototype.HideWait();
            }
            function getFlashVars_LangFileName() {
                return {
                    lang: "lang_fl_Ko.xml",
                    comLang: "lang_co_Ko.xml",
                    jobLang: "lang_bo_Ko.xml"
                };
            }
        </script>
        <link rel="stylesheet" type="text/css" href="default_blackwhite_skin.css">
        <link rel="stylesheet" type="text/css" href="default_user_skin.css">
        <title>박스</title>
    </head>
    <body>
        <input type="hidden" id="document_id" value="box_jobdetail">
        <input type="hidden" name="h_token" id="h_tkn" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
        <div id="Top" class="top-layout" style="display:none">
            <input type="hidden" id="PCM_FUNC_VER" value="10">
            <a href="#TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭으로 링크">
            </a>
            <div class="main-layout-top">
                <div class="main-layout-top-upper">
                    <div class="corp-logo">
                        <img src="horizon_s_SINDOH.gif" width="168" height="37" border="0" alt=" " class="corplogo-image">
                    </div>
                    <div class="topbutton-layout" cellpadding="0" cellspacing="0">
                        <div class="btn-logout">
                            <input type="button" class="logout-button2 btn-style-logout" id="Logout_Button" ALIGN="left" onclick="html_f.closeDialogAll(); html_f.showLogout();" value="로그인 화면">
                            <img src="login.png" alt=" " width="36" height="36" onclick="html_f.closeDialogAll(); html_f.showLogout();">
                        </div>
                        <div class="btn-change-password"></div>
                    </div>
                    <div class="logininfo">
                        <div class="logininfo-icon-layout" onclick="html_f.toggleMenuList(this, 'loginInfo-text', 'toggle-show');">
                            <input type="hidden" id="H_LoginMode" value="PublicUser">
                            <img src="b-user.png" alt=" " width="36" height="36">
                        </div>
                        <div id="loginInfo-text" class="logininfo-layout loginInfo-text" cellpadding="0" cellspacing="0">
                            <input type="hidden" id="loginMode" value="공유">
                            공유<input type="hidden" id="loginNameHidden" value="Public">
                        </div>
                    </div>
                </div>
                <div class="main-layout-top-bottom">
                    <div class="header-img">
                        <div class="tool-logo">
                            <img src="webconnection3_SINDOH.gif" width="189" height="25" border="0" alt=" ">
                        </div>
                        <div class="devicename-layout sindoh-layout" cellpadding="0" cellspacing="0">SINDOH D420</div>
                    </div>
                    <div class="contents-clear flash-layout">
                        <input type="hidden" name="PswcForm" id="PswcForm" value="">
                        <div id="StatusFlash" style="display:none;">
                            <div id="StatusFlashOn" style="display:none;"></div>
                            <div id="StatusFlashOff" style="display:none;">최신 Flah Player를 설치하십시오.</div>
                        </div>
                        <div id="StatusHtml" style="display:none;">
                            <script type="text/javascript" language="javascript" src="websocket_common.js"></script>
                            <script type="text/javascript" language="javascript" src="websocket_status.js"></script>
                            <table width="285px" border="0" cellspacing="0" id="DI_SCAN_TABLE" style="border-bottom-width:2px; border-bottom-style:solid; border-bottom-color:#3e3e3e">
                                <tr>
                                    <td width="35" bgcolor="#3e3e3e">
                                        <img id="DI_I_SS" alt=" " width="32" height="24" border="0" src="ScanRe.png">
                                    </td>
                                    <td width="155px" bgcolor="#3e3e3e">
                                        <font id="DI_T_SS" color="white">스캔할 준비가 되었습니다</font>
                                    </td>
                                    <td width="5" id="DI_T_SN"></td>
                                    <td width="90px" id="DI_T_SJ" style="color:white"></td>
                                </tr>
                            </table>
                            <table width="285px" border="0" cellspacing="0" id="DI_PRINT_TABLE" style="display:none;">
                                <tr>
                                    <td width="35" bgcolor="#3e3e3e" id="print-status-img" color="white">
                                        <a class="link-status-image" href="javascript:html_f.closeDialogAll();%20html_f.StatusJump('S_INF')">
                                            <img id="DI_I_PS" title="" alt=" " width="32" height="24" border="0" src="EngCa.png">
                                        </a>
                                    </td>
                                    <td width="155px" bgcolor="#3e3e3e" id="print-status-txt" color="white">
                                        <a class="link-status-text" href="javascript:html_f.closeDialogAll();%20html_f.StatusJump('S_INF');">
                                            <font id="DI_T_PS" color="white" bgcolor="#3e3e3e">용지 없음</font>
                                        </a>
                                    </td>
                                    <td width="5" id="DI_T_PN"></td>
                                    <td width="90px" id="DI_T_PJ" style="color:white"></td>
                                </tr>
                            </table>
                            <div id="StatusTemplate">
                                <script type="text/realtimeUpdate-template" value="link">
                                    <a class="link-status-text" href="javascript:html_f.closeDialogAll();html_f.StatusJump('#cookieid');">#content</a>
                                </script>
                            </div>
                        </div>
                    </div>
                    <div id="header-btn" class="contents-clear header-btn">
                        <div class="header-img">
                            <div class="tool-logo">
                                <img src="webconnection3_SINDOH.gif" width="189" height="25" border="0" alt=" ">
                            </div>
                            <div class="devicename-layout sindoh-layout" cellpadding="0" cellspacing="0">SINDOH D420</div>
                        </div>
                        <div class="btn-group">
                            <div class="btn-default dropdown-btn" id="dropdownBtn" onclick="html_f.toggleMenuList(this, 'dropdownMenu', 'toggle-show');">
                                <a href="javascript:;">
                                    <img src="mfp_icon1.png" width="24" height="24" border="0" alt="MFP情報検索/登録/表示">
                                </a>
                            </div>
                            <ul class="dropdown-menu" id="dropdownMenu">
                                <li class="dropdown-item icon-search" onclick="html_f.showModal(this,'MFP_SEARCH_MODAL');html_f.resetParam();html_f.getMfpList();">
                                    <input type="button" value="주변 MFP 검색">
                                </li>
                                <li class="dropdown-item icon-register" onclick="html_f.showModal(this,'MFP_REGISTER_MODAL');html_f.changeClickEvent();">
                                    <input type="button" value="MFP 정보 등록">
                                </li>
                                <li class="dropdown-item icon-list" onclick="html_f.showModal(this,'MFP_LOGIN_MODAL');">
                                    <input type="button" value="MFP 정보 표시">
                                </li>
                            </ul>
                        </div>
                        <div class="helpicon-layout" cellpadding="0" cellspacing="0">
                            <input type="hidden" id="OnlineHelpUrl" value="">
                            <a href="" id="helprelation" target="_blank" onclick="html_f.closeDialogAll(); html_f.setHelpPram('Ko')">
                                <img src="help.png" width="23" height="22" border="0" alt="수동">
                            </a>
                        </div>
                        <div class="refreshicon-layout" cellpadding="0" cellspacing="0">
                            <a href="javascript:html_f.closeDialogAll();%20html_f.updateUserPage();">
                                <img src="btn_refresh.png" width="24" height="24" border="0" alt="새로고침">
                            </a>
                        </div>
                        <div class="attentionicon-layout" cellpadding="0" cellspacing="0">
                            <div id="HddMirroringError_area" class="HddMirroringError_area">
                                <table border="0" cellspacing="0">
                                    <tr></tr>
                                </table>
                            </div>
                            <div id="networkErr_area" class="networkErr_area">
                                <table width="24" border="0" cellspacing="0">
                                    <tr>
                                        <img alt="" width="24" height="24" border="0" src="Network_NoError1.png">
                                        <input type="hidden" id="netWorkFlag" value="true">
                                        <input type="hidden" id="kmSaasgwFlag" value="true">
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="contents-clear moblie-status">
                        <div id="status-change-btn" class="status-change-btn" onclick="html_f.toggleMenuList(this, 'header-btn', 'toggle-show');">
                            <img width="36px" height="36px" border="0" src="mfp_ellipsis.png">
                        </div>
                    </div>
                </div>
                <div class="contents-clear menu-list-menu-back">
                    <div class="menu-list-btn">
                        <a href="javascript:;" id="menuListBtn" onclick="html_f.closeDialogAll(); html_f.toggleMenuList(this, 'mainMenu','toggle-show');">
                            <img src="tab-menu-icon1.png" alt="톱 메뉴" style="display:none">톱 메뉴
                        </a>
                    </div>
                    <div class="menu-back-btn">
                        <a href="javascript:;" id="menuBackBtn" onclick="html_f.closeDialogAll(); html_f.toggleMenuList(this, 'subMenu','toggle-show');">
                            <img src="tab-menu-icon2.png" alt="Sub 메뉴" style="display:none">Sub 메뉴
                        </a>
                    </div>
                </div>
            </div>
            <a href="#TabStartLink" name="TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭">
            </a>
            <a href="#MenuStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="부 메뉴로 링크">
            </a>
        </div>
        <div class="modal" id="MFP_REGISTER_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_REGISTER_FORM_CON">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            등록 번호 ： <label id="loginNumber" class="modal-number">1</label>
                            /20MFP 장치 수
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner" id="inner-register">
                            <div id="MFP_REGISTER_FORM">
                                <form class="form-content">
                                    <table class="table-normal" id="MFP_REGISTER_TAB">
                                        <tbody>
                                            <tr>
                                                <td width="35%">등록 번호</td>
                                                <td width="65%">
                                                    <label id="loginNo" class="modal-number">1</label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>IP 주소</td>
                                                <td>
                                                    <input type="text" id="loginIp" tabindex="1">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>기종명</td>
                                                <td>
                                                    <input type="text" id="loginName" tabindex="2">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <div class="text-formerror">
                                        <div id="InputDataStr_1" style="display:none">IP 주소 : 입력하지 않았습니다.</div>
                                        <div id="InputDataStr_2" style="display:none">IP 주소 : 입력 가능 문자수 초과</div>
                                        <div id="InputDataStr_3" style="display:none">기종명 : 입력하지 않았습니다.</div>
                                        <div id="InputDataStr_4" style="display:none">기종명 : 입력 가능 문자수 초과</div>
                                        <div id="InputDataStr_11" style="display:none">IP 주소 : 무효 글자 포함.</div>
                                    </div>
                                </form>
                            </div>
                            <div class="success-inner" id="MFP_DIALOG_SUC">
                                <div class="tip-area">
                                    <p class="tip-success">등록 완료</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="submit" class="btn  btn-primary" id="MFP_REGISTER_SUBMIT_BTN" onclick="html_f.chkInputFun(this);html_f.registerSubmit();" value="확인" tabindex="3">
                        <input type="submit" class="btn  btn-primary" id="MFP_REGISTER_SUBMIT_BTN1" onclick="html_f.chkInputFun(this);html_f.registerSubmit();" value="확인" tabindex="3">
                        <input type="button" id="MFP_REGISTER_FINISH_BTN" class="btn btn-default" onclick="html_f.closeModal('MFP_REGISTER_MODAL');" value="확인" tabindex="4">
                        <input type="button" class="btn btn-primary hide" id="MFP_REGISTER_CONTINU_BTN" onclick="html_f.goOnRegister();" value="계속 등록" tabindex="4">
                        <input type="button" class="btn btn-primary hide" id="MFP_OVER_BTN" onclick="html_f.closeModal('MFP_REGISTER_MODAL');html_f.showModal('','MFP_SEARCH_MODAL');html_f.DisplayNone('MFP_LOADING');html_f.DisplayBlock('MFP_SEARCH_LIST');" value="완료" tabindex="5">
                        <input type="button" id="MFP_REGISTER_CANCEL_BTN" class="btn btn-default" onclick="html_f.closeModal('MFP_REGISTER_MODAL');" value="취소" tabindex="3">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_WARNING">
                    <div class="modal-header">
                        <h4 class="modal-title">주의</h4>
                    </div>
                    <div class="modal-body">
                        <p class="iconfont icon-tip">입력값은 유효하지 않습니다.</p>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-primary" onclick="html_f.DisplayNone('MFP_DIALOG_WARNING');html_f.closeModal('MFP_REGISTER_MODAL');" value="닫기">
                    </div>
                </div>
            </div>
        </div>
        <div class="modal" id="MFP_LOGIN_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_LOGIN_LIST">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            MFP 정보 목록
                            <span class="page" id="MFP_LOGIN_PAGE">
                                <a href="javascript:;" class="pre" onclick="html_f.refreshCurrentPage('MFP_LOGIN_PAGE_PRE','MFP_LOGIN_MODAL');" id="MFP_LOGIN_PAGE_PRE">&lt;</a>
                                <span>
                                    <label title="current" id="MFP_LOGIN_PAGE_CUR">1</label>
                                    /
										<label title="total" id="MFP_LOGIN_PAGE_TOTAL">5</label>
                                </span>
                                <a href="javascript:;" class="next" onclick="html_f.refreshCurrentPage('MFP_LOGIN_PAGE_NEXT','MFP_LOGIN_MODAL');" id="MFP_LOGIN_PAGE_NEXT">&gt;</a>
                            </span>
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner">
                            <div class="login-list">
                                <div class="text-formerror table-err">
                                    <div id="InputDataStr_5" class="left-err" style="display:none">입력하지 않았습니다.</div>
                                    <div id="InputDataStr_6" class="left-err" style="display:none">입력 가능 문자수 초과</div>
                                    <div id="InputDataStr_7" class="right-err" style="display:none">입력하지 않았습니다.</div>
                                    <div id="InputDataStr_8" class="right-err" style="display:none">입력 가능 문자수 초과</div>
                                    <div id="InputDataStr_12" class="left-err" style="display:none">무효 글자 포함.</div>
                                </div>
                                <table border="0" cellspacing="0" class="table-interval" id="loginInfor">
                                    <thead>
                                        <tr>
                                            <th width="12%">등록 번호</th>
                                            <th width="30%">IP 주소</th>
                                            <th width="32%">기종명</th>
                                            <th width="26%">옵션</th>
                                        </tr>
                                    </thead>
                                    <tbody></tbody>
                                </table>
                                <div id="MFP_LOGIN_INFOR_OPTION_1" style="display:none;">
                                    <a href="javascript:;" onclick="html_f.loginInforEdit(this);">편집</a>
                                    <a href="javascript:;" onclick="html_f.loginInforDelete(this);">삭제</a>
                                </div>
                                <div id="MFP_LOGIN_INFOR_OPTION_2" style="display:none;">
                                    <a href="javascript:;" title="exsit" onclick="html_f.loginInforExsit(this);">저장</a>
                                    <a href="javascript:;" title="cancel" onclick="html_f.loginInforCancel(this);">취소</a>
                                </div>
                                <div id="MFP_DIALOG_SURE_DELETE" style="display:none;">
                                    <div class="to-sure-delete">
                                        <p>삭제하시겠습니까?</p>
                                        <input type="button" class="btn delete btn-primary" onclick="html_f.sureToDelete(this)" value="삭제">
                                        <input type="button" class="btn btn-default cancel" onclick="html_f.removeSureDelete();" value="취소">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="button" id="localWindowOpen" class="btn btn-default" onclick="html_f.sureLocalOpen('MFP_LOGIN_LIST','MFP_DIALOG_SURE');" value="이 화면에 표시">
                        <input type="button" class="btn btn-primary" id="newWindowOpen" onclick="html_f.newWindowOpen('MFP_LOGIN_MODAL');" value="다른 탭에 표시">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_LOGIN_MODAL');" value="닫기">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_SURE" style="display:none;">
                    <div class="modal-header"></div>
                    <div class="modal-body">
                        <h4 style="font-weight:normal;">이 화면에 표시하는 경우, 지금의 사용자나 관리자를 로그 아웃합니다. 계속하시겠습니까?</h4>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" id="backToSure" onclick="html_f.DisplayBlock('MFP_LOGIN_LIST');html_f.DisplayNone('MFP_DIALOG_SURE');" value="이전">
                        <input type="button" class="btn btn-primary" onclick="html_f.closeModal('MFP_LOGIN_MODAL');html_f.localOpen('MFP_LOGIN_MODAL');" value="이 화면에 표시">
                    </div>
                </div>
            </div>
        </div>
        <div class="modal" id="MFP_SEARCH_MODAL">
            <div class="modal-dialog">
                <div class="modal-content" id="MFP_SEARCH_LIST" style="display:none">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            주변 MFP 검색
                            <span class="page" id="MFP_SEARCH_PAGE">
                                <a href="javascript:;" class="pre" onclick="html_f.refreshCurrentPage('MFP_SEARCH_PAGE_PRE','MFP_SEARCH_MODAL');" id="MFP_SEARCH_PAGE_PRE">&lt;</a>
                                <span>
                                    <label title="current" id="MFP_SEARCH_PAGE_CUR">1</label>
                                    /
										<label title="total" id="MFP_SEARCH_PAGE_TOTAL">5</label>
                                </span>
                                <a href="javascript:;" class="next" onclick="html_f.refreshCurrentPage('MFP_SEARCH_PAGE_NEXT','MFP_SEARCH_MODAL');" id="MFP_SEARCH_PAGE_NEXT">&gt;</a>
                            </span>
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="drop-inner">
                            <div class="search-list">
                                <table border="0" cellspacing="0" class="table-interval table-has-radio" id="searchList">
                                    <thead>
                                        <tr>
                                            <th width="10%"></th>
                                            <th width="30%">기종명</th>
                                            <th width="40%">IP 주소</th>
                                            <th width="20%">옵션</th>
                                        </tr>
                                    </thead>
                                    <tbody></tbody>
                                </table>
                                <div id="MFP_SEARCH_LIST_OPTION">
                                    <a href="javascript:;" title="MFP 정보 등록" onclick="html_f.getIP(this);html_f.closeModal('MFP_SEARCH_MODAL');html_f.showModal('','MFP_REGISTER_MODAL');html_f.passByValue();html_f.DisplayNone('MFP_OVER_BTN');" class="icon_add"></a>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="button" id="MFP_SEARCH_RESULT_OPEN" class="btn btn-default" onclick="html_f.sureLocalOpen('MFP_SEARCH_LIST','MFP_DIALOG_SURE2');" value="이 화면에 표시">
                        <input type="button" id="MFP_NEW_RESULT_OPEN" class="btn btn-primary" onclick="html_f.newWindowOpen('MFP_SEARCH_MODAL');" value="다른 탭에 표시">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="닫기">
                    </div>
                </div>
                <div class="modal-content" id="MFP_LOADING">
                    <div class="modal-header">
                        <h4 class="modal-title">주변 MFP 검색</h4>
                    </div>
                    <div class="modal-body modal-body-loading">
                        <img width="32" height="32" src="load_wait.gif" alt="">MFP 정보 검색 중...
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="취소">
                    </div>
                </div>
                <div class="modal-content" id="MFP_SEARCH_FAIL" style="display:none;">
                    <div class="modal-header">
                        <h4 class="modal-title">주변 MFP 검색</h4>
                    </div>
                    <div class="modal-body">주변 MFP 검색에 실패하였습니다.</div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-primary" onclick="html_f.DisplayBlock('MFP_LOADING');html_f.DisplayNone('MFP_SEARCH_FAIL');html_f.resetParam();html_f.getMfpList();" value="재검색">
                        <input type="button" class="btn btn-default" onclick="html_f.closeModal('MFP_SEARCH_MODAL');" value="취소">
                    </div>
                </div>
                <div class="modal-content" id="MFP_DIALOG_SURE2" style="display:none;">
                    <div class="modal-header"></div>
                    <div class="modal-body">
                        <h4 style="font-weight:normal;">이 화면에 표시하는 경우, 지금의 사용자나 관리자를 로그 아웃합니다. 계속하시겠습니까?</h4>
                    </div>
                    <div class="modal-footer">
                        <input type="button" class="btn btn-default" onclick="html_f.DisplayBlock('MFP_SEARCH_LIST');html_f.DisplayNone('MFP_DIALOG_SURE2');" value="이전">
                        <input type="button" class="btn btn-primary" onclick="html_f.closeModal('MFP_SEARCH_MODAL');html_f.localOpen('MFP_SEARCH_MODAL');" value="이 화면에 표시">
                    </div>
                </div>
            </div>
        </div>
        <div id="MFP_LOGIN_LIST_FAIL" class="dialog-fail-alert"></div>
        <div id="MFP_COMMON_FAIL_MESSAGE" style="display:none;">시스템 오류가 발생하였습니다.</div>
        <div id="MFP_NO_RESULT" style="display:none;">검색 결과가 없습니다.</div>
        <div id="MainLayout" class="main-layout">
            <div id="mainMenu" class="main-menu-frame">
                <div id="Tab" class="tab-layout" style="display:none">
                    <div class="main-menu">
                        <ul class="main-menu-list-us">
                            <li class="tab tab-system-unselect" id="SystemLayout">
                                <a class="tab tab-unselect" href="system_device.xml" id="SystemFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="SystemIconImg" src="tab_system.png" alt="정보 표시" border="0" hspace="1px">
                                    <span>정보 표시</span>
                                </a>
                            </li>
                            <li class="tab tab-job-unselect" id="JobLayout">
                                <a class="tab tab-unselect" href="job_active.xml" id="JobFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="JobIconImg" src="tab_job.png" alt="작업" border="0" hspace="1px">
                                    <span>작업</span>
                                </a>
                            </li>
                            <li class="tab tab-box-unselect" id="BoxLayout">
                                <a class="tab tab-unselect" href="box_login.xml" id="FileFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="FileIconImg" src="tab_box.png" alt="박스" border="0" hspace="1px">
                                    <span>박스</span>
                                </a>
                            </li>
                            <li class="tab tab-print-unselect" id="PrintLayout">
                                <a class="tab tab-unselect" href="print.xml" id="PrintFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="PrintIconImg" src="tab_print.png" alt="다이렉트 인쇄" border="0" hspace="1px">
                                    <span>다이렉트 인쇄</span>
                                </a>
                            </li>
                            <li class="tab tab-abbr-unselect" id="AbbrLayout">
                                <a class="tab tab-unselect" href="abbr.xml" id="ScanFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="ScanIconImg" src="tab_abbr.png" alt="수신지 등록" border="0" hspace="1px">
                                    <span>수신지 등록</span>
                                </a>
                            </li>
                            <li class="tab tab-fav-unselect" id="FavLayout">
                                <a class="tab tab-unselect" href="favorite.xml" id="FavFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                                    <img id="FavIconImg" src="tab_favorite.png" alt="즐겨찾기 설정" border="0" hspace="1px">
                                    <span>즐겨찾기 설정</span>
                                </a>
                            </li>
                            <li class="tab-top-custom tab-custom-unselect" id="CustomLayout">
                                <a class="tab tab-unselect" href="custom.xml" id="CustomFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();html_f.setMainMuneCookie();">
                                    <img id="CustomIconImg" src="tab_custom.png" alt="사용자 설정" border="0" hspace="1px">
                                    <span>사용자 설정</span>
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                <div id="Tab-Border" style="display:none"></div>
            </div>
            <div id="subMenu" class="sub-menu-frame">
                <div id="SS3" class="menu-layout" style="display:none">
                    <a href="#ContentsStartLink">
                        <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차로 링크">
                    </a>
                    <div class="menu-box">
                        <div class="menu-main" id="M_BoxLogin">
                            <a id="BoxLogin" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_UOU');html_f.menuLocation('box_login.xml');" style="display:block">
                                <div class="menu-text">박스 열기</div>
                            </a>
                            <button id="BoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_UOU');"></button>
                        </div>
                        <div class="menu-main" id="M_BoxList">
                            <a id="BoxList" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_ULU');html_f.menuLocation('box_list.xml');" style="display:block">
                                <div class="menu-text">박스 목록</div>
                            </a>
                            <button id="BoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_ULU');"></button>
                        </div>
                        <div class="menu-main" id="M_SysBoxLogin">
                            <a id="SysBoxLogin" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_SOU');html_f.menuLocation('box_slogin.xml');" style="display:block">
                                <div class="menu-text">시스템 박스 열기</div>
                            </a>
                            <button id="SysBoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SOU');"></button>
                        </div>
                        <div class="menu-main" id="M_SysBoxList">
                            <a id="SysBoxList" class="menu menu-unselect" href="javascript:html_f.closeDialogAll();html_f.setCookie('F_SLU');html_f.menuLocation('box_slist.xml');" style="display:block">
                                <div class="menu-text">시스템 박스 목록</div>
                            </a>
                            <button id="SysBoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SLU');"></button>
                        </div>
                    </div>
                    <a name="ContentsStartLink" href="#ContentsStartLink">
                        <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차">
                    </a>
                </div>
            </div>
            <div id="Main" class="body-layout" style="display:none">
                <a href="#PageEndLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="페이지 종료로 링크">
                </a>
                <div class="contents-box">
                    <div id="F_UOU" class="contents-layout" style="display:none;">
                        <input type="hidden" id="$HELP_ID" value="?Menu=UBox_AA@Contents=menuUserDABE">
                        <form method="POST" id="F_UOU_CH" name="F_UOU_CHN" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOU_CHN">
                            <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_JOB" value="1">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_USR" value="">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_NAM" value="TEST_SCAN">
                            <input type="hidden" name="H_SAV" value="0">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_PID" value="">
                            <input type="hidden" name="H_DTY" value="AllDocument">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_ALN" id="F_UOU_H_ALN" value="다음">
                            <input type="hidden" name="H_ALB" id="F_UOU_H_ALB" value="이전">
                            <input type="hidden" name="H_TAB" id="H_F_F_UOU_CH" value="">
                            <input type="hidden" id="AppPreview" value="On">
                            <h1 class="contents-header">문서 상세 정보</h1>
                            <div class="contents-body">
                                <div class="contents-clear">
                                    <div class="jobdetail-thumbnail">
                                        <table>
                                            <tr>
                                                <td indent="td-indent1"></td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px" height="160px">
                                                        <tr>
                                                            <td width="18px">
                                                                <div id="F_UOU_B_BCK"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td>
                                                    <table class="thumbnail-table2" width="160px">
                                                        <tr>
                                                            <td width="160px" height="160px" align="center">
                                                                <input type="hidden" name="P_TYP" id="F_UOU_PTP" value="Normal">
                                                                <input type="hidden" name="H_BID" id="F_UOU_BID" value="3">
                                                                <input type="hidden" name="H_BID" id="F_UOU_BTY" value="User">
                                                                <input type="hidden" name="H_JID" id="F_UOU_JID" value="1">
                                                                <input type="hidden" name="H_DNB" id="F_UOU_DNB" value="1">
                                                                <div id="F_UOU_BTI"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td>
                                                    <table width="18px" height="160px">
                                                        <tr>
                                                            <td width="18px" valign="center">
                                                                <div id="F_UOU_B_NXT"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px"></table>
                                                </td>
                                                <td align="center">
                                                    <table class="thumbnail_button-table">
                                                        <tr>
                                                            <td>
                                                                <div id="F_UOU_PGA">
                                                                    페이지 <input type="text" id="F_UOU_PNB" name="H_PNB" size="4" maxlength="4" value="1">
                                                                    <input type="button" value="Go" onclick="javascript:box_jobdetail_f.ChkPage('F_UOU');">
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px"></table>
                                                </td>
                                                <td align="center">
                                                    <input type="button" value="미리보기 상세" id="F_UOU_Preview" onclick="box_jobdetail_f.setPreviewParam('cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy',3,1,'User','1');               box_jobdetail_f.openDialog('box_preview_post.html');">
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <div class="jobdetail-thumbnail">
                                        <table class="data-table-noline" width="270px">
                                            <tr>
                                                <td scope="row" width="130px">문서 이름</td>
                                                <td width="160px">
                                                    <input type="text" name="T_JNA" id="F_UOU_JNA" size="20px" maxlength="30" value="S25C-926042414080">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">사용자 이름</td>
                                                <td width="160px">SCAN</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">등록 시간</td>
                                                <td width="160px">24/04/2026 14:08</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">원고 매수</td>
                                                <td width="160px">1</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">컬러 종류</td>
                                                <td width="160px">풀컬러</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">파일 형식</td>
                                                <td width="160px">압축 PDF</td>
                                            </tr>
                                        </table>
                                    </div>
                                </div>
                                <hr class="contents-end" title="">
                                <div class="buttonarea-layout" id="comButtonAreaJobDetail">
                                    <input type="button" value="확인" id="F_UOU_Apply2" onclick="javascript:if(box_jobdetail_f.ChkComFunc())box_jobdetail_f.FormSubmit('F_UOU_CH');" class="button-height btn-style-bottom btn-style-ok">
                                    <input type="button" value="취소" id="F_UOU_Cancel1" onclick="box_jobdetail_f.btnDisabled();box_jobdetail_f.FormSubmit('F_UOU_CC');" class="button-height btn-style-bottom">
                                    <input type="reset" id="F_UOU_C_DO" value="지우기" onclick="box_jobdetail_f.ClearErr();box_jobdetail_f.formReset('F_UOU_CH');" style="display:none" class="btn-style-bottom">
                                </div>
                                <div class="favorite-buttonarea-layout" id="favButtonAreaJobDetail" style="display:none">
                                    <input type="button" value="확인" id="F_UOU_Apply2" onclick="javascript:if(box_jobdetail_f.ChkComFunc())box_jobdetail_f.FormSubmit('F_UOU_CH');" class="button-height btn-style-bottom btn-style-ok">
                                    <input type="button" value="취소" id="F_UOU_Cancel1" onclick="box_jobdetail_f.btnDisabled();box_jobdetail_f.FormSubmit('F_UOU_CC');" class="button-height btn-style-bottom">
                                    <input type="reset" id="F_UOU_C_DO" value="지우기" onclick="box_jobdetail_f.ClearErr();box_jobdetail_f.formReset('F_UOU_CH');" style="display:none" class="btn-style-bottom">
                                </div>
                                <div class="text-formerror-title">
                                    <div id="F_UOU_ERRTitle" style="display:none">오류</div>
                                </div>
                                <div class="text-formerror">
                                    <div id="F_UOU_JNAStr_1" style="display:none">문서 이름 : 입력하지 않았습니다.</div>
                                    <div id="F_UOU_JNAStr_2" style="display:none">문서 이름 : 입력 가능 문자수 초과</div>
                                    <div id="F_UOU_JNAStr_3" style="display:none">문서 이름 : 무효 글자 포함.</div>
                                    <div id="F_UOU_PNBNum_1" style="display:none">페이지 : 입력하지 않았습니다.</div>
                                    <div id="F_UOU_PNBNum_2" style="display:none">페이지 : 숫자 이외의 문자 포함.</div>
                                    <div id="F_UOU_PNBNum_3" style="display:none">페이지 : 입력값은 유효하지 않습니다.</div>
                                </div>
                                <div class="bottom-area"></div>
                            </div>
                        </form>
                        <form method="POST" id="F_UOU_CC" name="F_UOU_CCL" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOU_CCL">
                            <input type="hidden" name="h_token" id="h_tkn" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_PID" value="">
                            <input type="hidden" name="H_DTY" value="AllDocument">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_TAB" id="H_F_F_UOU_CC" value="">
                        </form>
                        <form id="F_UOU_FD" name="F_UOU_FDL" method="post" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOU_FDL">
                            <input type="hidden" name="h_token" id="h_tkn" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_JOB" value="1">
                            <input type="hidden" name="H_CNT" value="1">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_OPE" value="AllDocument">
                            <input type="hidden" name="H_POS" value="01">
                            <input type="hidden" name="S_PAG" value="">
                        </form>
                    </div>
                    <div id="F_UOA" class="contents-layout" style="display:none;">
                        <input type="hidden" id="$HELP_ID" value="?Menu=UBox_AA@Contents=menuUserDABE">
                        <form method="POST" id="F_UOA_CH" name="F_UOA_CHN" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOA_CHN">
                            <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_JOB" value="1">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_USR" value="">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_NAM" value="TEST_SCAN">
                            <input type="hidden" name="H_SAV" value="0">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_PID" value="">
                            <input type="hidden" name="H_DTY" value="AllDocument">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_ALN" id="F_UOA_H_ALN" value="다음">
                            <input type="hidden" name="H_ALB" id="F_UOA_H_ALB" value="이전">
                            <input type="hidden" name="H_TAB" id="H_F_F_UOA_CH" value="">
                            <input type="hidden" id="AppPreview" value="On">
                            <h1 class="contents-header">문서 상세 정보</h1>
                            <div class="contents-body">
                                <div class="contents-clear">
                                    <div class="jobdetail-thumbnail">
                                        <table>
                                            <tr>
                                                <td indent="td-indent1"></td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px" height="160px">
                                                        <tr>
                                                            <td width="18px">
                                                                <div id="F_UOA_B_BCK"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td>
                                                    <table class="thumbnail-table2" width="160px">
                                                        <tr>
                                                            <td width="160px" height="160px" align="center">
                                                                <input type="hidden" name="P_TYP" id="F_UOA_PTP" value="Normal">
                                                                <input type="hidden" name="H_BID" id="F_UOA_BID" value="3">
                                                                <input type="hidden" name="H_BID" id="F_UOA_BTY" value="User">
                                                                <input type="hidden" name="H_JID" id="F_UOA_JID" value="1">
                                                                <input type="hidden" name="H_DNB" id="F_UOA_DNB" value="1">
                                                                <div id="F_UOA_BTI"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td>
                                                    <table width="18px" height="160px">
                                                        <tr>
                                                            <td width="18px" valign="center">
                                                                <div id="F_UOA_B_NXT"></div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px"></table>
                                                </td>
                                                <td align="center">
                                                    <table class="thumbnail_button-table">
                                                        <tr>
                                                            <td>
                                                                <div id="F_UOA_PGA">
                                                                    페이지 <input type="text" id="F_UOA_PNB" name="H_PNB" size="4" maxlength="4" value="1">
                                                                    <input type="button" value="Go" onclick="javascript:box_jobdetail_f.ChkPage('F_UOA');">
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <table width="18px"></table>
                                                </td>
                                                <td align="center">
                                                    <input type="button" value="미리보기 상세" id="F_UOA_Preview" onclick="box_jobdetail_f.setPreviewParam('cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy',3,1,'User','1');               box_jobdetail_f.openDialog('box_preview_post.html');">
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <div class="jobdetail-thumbnail">
                                        <table class="data-table-noline" width="270px">
                                            <tr>
                                                <td scope="row" width="130px">문서 이름</td>
                                                <td width="160px">
                                                    <input type="text" name="T_JNA" id="F_UOA_JNA" size="20px" maxlength="30" value="S25C-926042414080">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">사용자 이름</td>
                                                <td width="160px">SCAN</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">등록 시간</td>
                                                <td width="160px">24/04/2026 14:08</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">원고 매수</td>
                                                <td width="160px">1</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">컬러 종류</td>
                                                <td width="160px">풀컬러</td>
                                            </tr>
                                            <tr>
                                                <td scope="row" width="130px">파일 형식</td>
                                                <td width="160px">압축 PDF</td>
                                            </tr>
                                        </table>
                                    </div>
                                </div>
                                <hr class="contents-end" title="">
                                <div class="buttonarea-layout" id="comButtonAreaJobDetail">
                                    <input type="button" value="확인" id="F_UOA_Apply2" onclick="javascript:if(box_jobdetail_f.ChkComFunc())box_jobdetail_f.FormSubmit('F_UOA_CH');" class="button-height btn-style-bottom btn-style-ok">
                                    <input type="button" value="취소" id="F_UOA_Cancel1" onclick="box_jobdetail_f.btnDisabled();box_jobdetail_f.FormSubmit('F_UOA_CC');" class="button-height btn-style-bottom">
                                    <input type="reset" id="F_UOA_C_DO" value="지우기" onclick="box_jobdetail_f.ClearErr();box_jobdetail_f.formReset('F_UOA_CH');" style="display:none" class="btn-style-bottom">
                                </div>
                                <div class="favorite-buttonarea-layout" id="favButtonAreaJobDetail" style="display:none">
                                    <input type="button" value="확인" id="F_UOA_Apply2" onclick="javascript:if(box_jobdetail_f.ChkComFunc())box_jobdetail_f.FormSubmit('F_UOA_CH');" class="button-height btn-style-bottom btn-style-ok">
                                    <input type="button" value="취소" id="F_UOA_Cancel1" onclick="box_jobdetail_f.btnDisabled();box_jobdetail_f.FormSubmit('F_UOA_CC');" class="button-height btn-style-bottom">
                                    <input type="reset" id="F_UOA_C_DO" value="지우기" onclick="box_jobdetail_f.ClearErr();box_jobdetail_f.formReset('F_UOA_CH');" style="display:none" class="btn-style-bottom">
                                </div>
                                <div class="text-formerror-title">
                                    <div id="F_UOA_ERRTitle" style="display:none">오류</div>
                                </div>
                                <div class="text-formerror">
                                    <div id="F_UOA_JNAStr_1" style="display:none">문서 이름 : 입력하지 않았습니다.</div>
                                    <div id="F_UOA_JNAStr_2" style="display:none">문서 이름 : 입력 가능 문자수 초과</div>
                                    <div id="F_UOA_JNAStr_3" style="display:none">문서 이름 : 무효 글자 포함.</div>
                                    <div id="F_UOA_PNBNum_1" style="display:none">페이지 : 입력하지 않았습니다.</div>
                                    <div id="F_UOA_PNBNum_2" style="display:none">페이지 : 숫자 이외의 문자 포함.</div>
                                    <div id="F_UOA_PNBNum_3" style="display:none">페이지 : 입력값은 유효하지 않습니다.</div>
                                </div>
                                <div class="bottom-area"></div>
                            </div>
                        </form>
                        <form method="POST" id="F_UOA_CC" name="F_UOA_CCL" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOA_CCL">
                            <input type="hidden" name="h_token" id="h_tkn" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_PID" value="">
                            <input type="hidden" name="H_DTY" value="AllDocument">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_TAB" id="H_F_F_UOA_CC" value="">
                        </form>
                        <form id="F_UOA_FD" name="F_UOA_FDL" method="post" action="user.cgi" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_F_UOA_FDL">
                            <input type="hidden" name="h_token" id="h_tkn" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
                            <input type="hidden" name="H_BOX" value="3">
                            <input type="hidden" name="H_BPA" value="">
                            <input type="hidden" name="H_BTY" value="User">
                            <input type="hidden" name="H_JOB" value="1">
                            <input type="hidden" name="H_CNT" value="1">
                            <input type="hidden" name="H_XTP" value="ListDisp">
                            <input type="hidden" name="H_OPE" value="AllDocument">
                            <input type="hidden" name="H_POS" value="01">
                            <input type="hidden" name="S_PAG" value="">
                        </form>
                    </div>
                </div>
                <a name="PageEndLink" href="#PageEndLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="페이지 종료">
                </a>
            </div>
            <div class="contents-box">
                <div class="body-layout" style="display:block;">
                    <div id="LowBatt" class="contents-layout" style="display:none;">
                        <input type="hidden" id="HELP_LOWBATT" value="?Menu=top@Contents=LowBatt">
                        <form id="AS_LB" name="AS_LOW" method="POST" action="user.cgi" onsubmit="html_f.setCgiURL('H_URL_LOWBATT');html_f.btnDisabled();html_f.contentsButtonDisabled(id);" Accept-charset="UTF-8">
                            <input type="hidden" name="func" value="PSL_D_PSV">
                            <input type="hidden" name="H_JOB" id="H_JOB_LOWBATT" value="">
                            <input type="hidden" name="H_PLS" id="H_PLS_LOWBATT" value="">
                            <input type="hidden" name="H_URL" id="H_URL_LOWBATT" value="">
                            <h1 class="contents-header">절전 모드</h1>
                            <P>
                            <div class="contents-body">
                                <div id="NoLowBattOnJobOnAdmin" style="display:none;">절전모드로 이동할 수 없습니다. 장치가 관리자 모드이거나, 진행중인 작업이 있습니다.</div>
                                <div id="NoLowBattOnJob" style="display:none;">절전모드로 이동할 수 없습니다. 진행중인 작업이 있습니다.</div>
                                <div id="NoLowBattOnAdmin" style="display:none;">절전모드로 이동할 수 없습니다. 장치가 관리자 모드이거나, 작업이 진행중입니다.</div>
                                <div id="ConfLowBattOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중입니다. 절전 모드로 이동하시겠습니까?</div>
                                <div id="ConfLowBatt" style="display:none;">절전 모드로 이동하시겠습니까?</div>
                            </div>
</P><hr class="contents-end" title="종료">
<div class="buttonarea-layout">
    <div id="LowBattOK" style="display:none;">
        <input type="submit" value="확인" class="button-height">
        <input type="button" value="취소" onclick="html_f.ClearLowBattery()" class="button-height">
    </div>
    <div id="LowBattNG" style="display:none;">
        <input type="button" value="확인" onclick="html_f.ClearLowBattery()">
    </div>
</div>
</form></div>
<div id="Logout" class="contents-layout" style="display:none;">
    <input type="hidden" id="HELP_LOGOUT" value="?Menu=top@Contents=menuLoginB">
    <form id="AS_LO" name="AS_LGO" method="POST" action="user.cgi" onsubmit="html_f.closeDialogAll();html_f.btnDisabled();html_f.contentsButtonDisabled(id);html_f.resetAbbrCheckCookie();" Accept-charset="UTF-8">
        <input type="hidden" name="func" value="PSL_ACO_LGO">
        <input type="hidden" name="h_token" value="cZn5CgFiwiNggi2wOw6fNrnqHTaRILKy">
        <h1 class="contents-header">로그아웃</h1>
        <div class="contents-body">
            <p>공유 사용자로부터 로그아웃 하고, 로그인 화면으로 이동하시겠습니까?</p>
        </div>
        <hr class="contents-end" title="종료">
        <div class="buttonarea-layout">
            <input type="submit" value="확인" class="button-height btn-style-bottom btn-style-ok">
            <input type="button" value="취소" onclick="html_f.ComReturnDisplay()" class="button-height btn-style-bottom">
        </div>
        <div class="bottom-area"></div>
    </form>
</div>
<div id="Reboot" class="contents-layout" style="display:none;">
    <input type="hidden" id="HELP_REBOOT" value="?Menu=top@Contents=Reboot">
    <form id="AS_RB" name="AS_REB" method="POST" action="a_user.cgi" onsubmit="html_f.setCgiURL('H_URL_REBOOT');html_f.btnDisabled();html_f.contentsButtonDisabled(id);" Accept-charset="UTF-8">
        <input type="hidden" name="func" value="PSL_D_REB">
        <input type="hidden" name="H_JOB" id="H_JOB_REBOOT" value="">
        <input type="hidden" name="H_PLS" id="H_PLS_REBOOT" value="">
        <input type="hidden" name="H_URL" id="H_URL_REBOOT" value="">
        <h1 class="contents-header">재시작</h1>
        <P>
        <div class="contents-body">
            <div id="ConfRebootOnJobOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중이고, 실행중인 작업이 있습니다.재기동을 하시겠습니까?</div>
            <div id="ConfRebootOnJob" style="display:none;">실행중인 작업이 있습니다. 재기동을 하시겠습니까?</div>
            <div id="ConfRebootOnUser" style="display:none;">사용자가 터치 패널 메뉴에 접속중입니다. 재기동을 하시겠습니까?</div>
            <div id="ConfReboot" style="display:none;">재기동을 하시겠습니까?</div>
        </div>
</P><hr class="contents-end" title="종료">
<div class="buttonarea-layout">
    <input type="submit" value="확인" class="button-height">
    <input type="button" value="취소" onclick="html_f.ClearReboot()" class="button-height">
</div>
</form></div></div></div></div><iframe src="box_jobdetail_f.html" name="box_jobdetail_f" width="0" height="0" frameborder="0"></iframe>
<iframe src="box_jobdetail_f.html" name="html_f" width="0" height="0" frameborder="0"></iframe>
<iframe name="livecheck" id="livecheck" src="livecheck.html" frameborder="0" height="0" width="0"></iframe>
</body></html>

## 파일 보기
Request URL
http://192.168.11.241/wcd/thumbnail.jpg?func=PSL_F_PREVIEW&h_token=&H_BOX=&H_BTY=&H_JOB=&H_PAG=
페이로드: func=PSL_F_PREVIEW&h_token=&H_BOX=&H_BTY=&H_JOB=&H_PAG=
응답: 
* 프리뷰 팝업창에서 새로고침 시, 세션 503 발생. 일정 시간 대기 후 해제됨0

## 파일 다운로드 클릭 (파일 다운로드 설정 창 진입)
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
Status Code
200 OK
페이로드: func=PSL_F_UOU_NXT&h_token=&H_JLS=%401%40&H_CLS=&H_PID=-1&H_DTY=FileDownload&H_XTP=Thumbnail&H_TAB=&H_BOX=3&H_BPA=&H_BTY=User&H_OPE=FileDownload&H_MAX=1

## 설정 후 다운로드 클릭 (다운로드 대기 창 진입)
Request URL
http://192.168.11.241/wcd/user.cgi
Request Method
POST
Status Code
200 OK
페이로드: H_TAB=&func=PSL_F_UOU_DWN&h_token=IAvhw4waexfu3QFm82Vcqedcp1yJYPs7&H_BOX=3&H_BPA=&H_BTY=User&H_PID=-1&H_DTY=FileDownload&H_XTP=Thumbnail&H_FMT=CompactPdf&H_JLS=%401%40&H_JNL=%09S25C-926042414080%09&H_JOR=%401%40&H_DCN=1&C_GFA=On&C_SET=C_SET&F_UOU_S_FOR=CompactPdf&S_OUT=Off&F_UOU_R_PAG=MultiPage&R_SPG=Off

## 다운로드중
Request URL
http://192.168.11.241/wcd/progress
Request Method
GET
Status Code
200 OK

응답: <HTML xmlns:xlink="http://www.w3.org/1999/xlink" lang="en">
    <HEAD>
        <meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta content="text/javascript" http-equiv="Content-Script-Type">
        <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=yes">
        <link rel="stylesheet" type="text/css" href="default_blackwhite_skin.css">
        <link rel="stylesheet" type="text/css" href="default_user_skin.css">
        <TITLE>Result</TITLE>
    </HEAD>
    <BODY LINK="#000000" ALINK="#ff0000" VLINK="#000000">
        <input type="hidden" id="DN74B7" value="Off">
        <input type="hidden" id="BoxType" value="">
        <FORM id="A_DL" name="A_DLD" method="POST" action="doc/S25C-926042414080.pdf">
            <input type="hidden" name="func" value="PSL_F_UOU_DLD">
            <input type="hidden" name="h_token" value="IAvhw4waexfu3QFm82Vcqedcp1yJYPs7">
            <input type="hidden" name="cginame1" id="H_CGI_N" value="doc/S25C-926042414080.pdf">
            <input type="hidden" name="cginame2" id="H_CGI_R" value="doc/S25C-926042414080.pdf">
            <input type="hidden" name="H_BAK" id="H_BA" value="0">
            <input type="hidden" name="H_TAB" id="H_F_A_DL" value="">
            <input type="hidden" name="H_DLV" id="H_DL" value="">
            <table valign="top" width="510px" align="center">
                <tr>
                    <th height="50px" align="left"></th>
                </tr>
                <tr>
                    <td width="510px" colspan="3">
                        다운로드 데이터를 작성할 수 있습니다. "다운로드"버튼을 눌러, 저장을 시작하십시오. 저장이 종료되면, "이전"버튼을 눌러 주십시오.<br>
                    </td>
                </tr>
                <tr>
                    <td width="510px" colspan="3">
                        <HR class="Line">
                    </td>
                </tr>
                <tr>
                    <td width="350px" colspan="1"></td>
                    <td colspan="1">
                        <INPUT type="button" id="btnEXE" value="다운로드" onclick="document.getElementById('download_f').contentWindow.BInvisible('btnEXE');document.getElementById('download_f').contentWindow.executeExport();" style="display:none">
                    </td>
                    <td colspan="1">
                        <INPUT type="button" id="downloadbtnOK" value="이전" onclick="document.getElementById('download_f').contentWindow.BDisabled('btnEXE');document.getElementById('download_f').contentWindow.BDisabled('downloadbtnOK');document.getElementById('download_f').contentWindow.MoveUrl('box_login.xml')" style="display:none">
                    </td>
                </tr>
            </table>
        </FORM>
        <iframe name="livecheck" id="livecheck" src="livecheck.html" frameborder="0" height="0" width="0"></iframe>
        <iframe name="download_f" id="download_f" src="download_f.html" frameborder="0" height="0" width="0"></iframe>
        <input type="button" id="download_href" style="display:none" onclick="parent.location.href = 'box_login.xml'">
    </BODY>
</HTML>

## 로딩 후, 다운로드 클릭
Request URL
http://192.168.11.241/wcd/doc/S25C-926042414080.pdf
Request Method
POST
Status Code
200 OK
페이로드: func=PSL_F_UOU_DLD&h_token=IAvhw4waexfu3QFm82Vcqedcp1yJYPs7&cginame1=doc%2FS25C-926042414080.pdf&cginame2=doc%2FS25C-926042414080.pdf&H_BAK=0&H_TAB=&H_DLV=


