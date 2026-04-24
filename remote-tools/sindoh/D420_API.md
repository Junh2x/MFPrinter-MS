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
