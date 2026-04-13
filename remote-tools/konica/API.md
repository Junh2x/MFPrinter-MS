## 박스 목록 조회
Request URL
http://192.168.11.112/wcd/box_list.xml
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.112:80
Referrer Policy
strict-origin-when-cross-origin
content-length
11071
content-type
text/xml
expires
Thu, 01 Jan 1970 00:00:00 GMT
x-frame-options
SAMEORIGIN
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; param=; access=; bm=Low; abbrRedojobingStatus=allow; selno=Ko; wd=n; help=off,off,off; adm=AS_COU; ver_expires=Tue, 13 Apr 2027 10:48:25 GMT; ID=AVtNYoFbvQjXgyUJnvYK5Oyhfa98zyuJ; abbrCheckCookieFlg=true; loginUserName=Public; usr=F_ULU
host
192.168.11.112
referer
http://192.168.11.112/wcd/box_login.xml
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36

응답:
<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta content="text/javascript" http-equiv="Content-Script-Type">
        <script type="text/javascript" charset="utf-8">
            function getFlashVars_Status() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User&ExternalIfMode=None"
            }
            function getFlashVars_InputTray() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User"
            }
            function getFlashVars_ActiveJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User&Favorite=" + Favorite + ""
            }
            function getFlashVars_DoneJob() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User"
            }
            function getFlashVars_Commlist() {
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User"
            }
            function getFlashVars_AccumulationJob() {
                var Favorite = 0;
                if (parent.document.getElementById("H_FAV_FLG")) {
                    Favorite = 1;
                }
                return "LangFile_xslt=lang_fl_Ko.xml&ClientCnt_xslt=90&UserMode=User&Favorite=" + Favorite + ""
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
        <link rel="stylesheet" type="text/css" href="default.css">
        <link rel="stylesheet" type="text/css" href="box.css">
        <link rel="stylesheet" type="text/css" href="default_user_skin.css">
        <style type="text/css" id="tempCssId"></style>
        <title>박스 - PageScope Web Connection</title>
    </head>
    <body>
        <input type="hidden" id="document_id" value="box">
        <div id="Top" class="top-layout" style="display:none">
            <input type="hidden" id="PCM_FUNC_VER" value="8">
            <a href="#TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭으로 링크">
            </a>
            <div class="logo-layout" cellpadding="0" cellspacing="0">
                <div>
                    <a href="http://konicaminolta.com/" target="_blank">
                        <img src="horizon_s.gif" width="168" height="37" border="0" alt="KONICA MINOLTA 로고 홈페이지로의 링크">
                    </a>
                </div>
                <div>
                    <a href="javascript:html_f.about('about.html')">
                        <img src="webconnection.gif" width="206" height="26" border="0" alt="제품 로고 PageScopeWebConnection About 정보로의 링크">
                    </a>
                </div>
            </div>
            <div class="devicename-layout" cellpadding="0" cellspacing="0">기종명:bizhub C258</div>
            <div class="logininfo-icon-layout">
                <input type="hidden" id="H_LoginMode" value="PublicUser">
                <img src="user.gif" alt=" ">
            </div>
            <div class="logininfo-layout" cellpadding="0" cellspacing="0">
                <input type="hidden" id="loginMode" value="공유">
                공유<input type="hidden" id="loginNameHidden" value="Public">
            </div>
            <div class="topbutton-layout" cellpadding="0" cellspacing="0">
                <input type="button" class="logout-button2" id="Logout_Button" ALIGN="left" onclick="html_f.closeDialogAll(); html_f.showLogout();" value="로그인 화면">
            </div>
            <div class="helpicon-layout" cellpadding="0" cellspacing="0">
                <input type="hidden" id="OnlineHelpUrl" value="http://www.pagescope.com/download/webconnection/onlinehelp/C258/help.html">
                <a href="" id="helprelation" target="_blank" onclick="html_f.closeDialogAll(); html_f.setHelpPram('Ko')">
                    <img src="help.gif" width="23" height="22" border="0" alt="수동">
                </a>
            </div>
            <div class="flash-layout">
                <input type="hidden" name="PswcForm" id="PswcForm" value="">
                <div id="StatusFlash" style="display:none;">
                    <div id="StatusFlashOn" style="display:none;"></div>
                    <div id="StatusFlashOff" style="display:none;">최신 Flah Player를 설치하십시오.</div>
                </div>
                <div id="StatusHtml" style="display:none;">
                    <script type="text/javascript" language="javascript" src="websocket_common.js"></script>
                    <script type="text/javascript" language="javascript" src="websocket_status.js"></script>
                    <table width="400" border="0" cellspacing="0" style="border-bottom-width:2px; border-bottom-style:solid; border-bottom-color:white">
                        <tr>
                            <td width="35" bgcolor="white">
                                <img id="DI_I_SS" alt=" " width="32" height="24" border="0" src="ScanRe.gif">
                            </td>
                            <td width="240" bgcolor="white">
                                <font id="DI_T_SS" color="black">스캔할 준비가 되었습니다</font>
                            </td>
                            <td width="5" id="DI_T_SN"></td>
                            <td width="120" id="DI_T_SJ"></td>
                        </tr>
                    </table>
                    <table width="400" border="0" cellspacing="0">
                        <tr>
                            <td width="35" bgcolor="white" id="print-status-img">
                                <img id="DI_I_PS" alt=" " width="32" height="24" border="0" src="EngRe.gif">
                            </td>
                            <td width="240" bgcolor="white" id="print-status-txt">
                                <font id="DI_T_PS" color="black">인쇄할 준비가 되었습니다.</font>
                            </td>
                            <td width="5" id="DI_T_PN"></td>
                            <td width="120" id="DI_T_PJ"></td>
                        </tr>
                    </table>
                    <div id="StatusTemplate">
                        <script type="text/realtimeUpdate-template" value="link">
                            <a class="link-status-text" href="javascript:html_f.closeDialogAll();html_f.StatusJump('#cookieid');">#content</a>
                        </script>
                    </div>
                </div>
            </div>
            <div class="attentionicon-layout" cellpadding="0" cellspacing="0">
                <div id="HddMirroringError_area" class="HddMirroringError_area">
                    <table width="40" border="0" cellspacing="0">
                        <tr></tr>
                    </table>
                </div>
                <div id="networkErr_area" class="networkErr_area">
                    <table width="40" border="0" cellspacing="0">
                        <tr>
                            <img alt=" " width="24" height="24" border="0" src="Network_NoError.png">
                            <input type="hidden" id="netWorkFlag" value="true">
                            <input type="hidden" id="kmSaasgwFlag" value="true">
                        </tr>
                    </table>
                </div>
            </div>
            <div class="refreshicon-layout" cellpadding="0" cellspacing="0">
                <a href="javascript:html_f.closeDialogAll();%20html_f.updateUserPage();">
                    <img src="btn_refresh.gif" width="24" height="24" border="0" alt="새로고침">
                </a>
            </div>
            <div class="header-border"></div>
            <a href="#TabStartLink" name="TabStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="기능 탭">
            </a>
            <a href="#MenuStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="부 메뉴로 링크">
            </a>
        </div>
        <table id="Tab" class="tab-layout" style="display:none">
            <tr>
                <td class="tab tab-system-unselect" id="SystemLayout">
                    <a class="tab tab-unselect" href="system_device.xml" id="SystemFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="SystemIconImg" src="tab_system2.gif" alt="정보 표시" border="0" hspace="1px">
                        </span>
                        정보 표시
                    </a>
                </td>
                <td class="tab tab-job-unselect" id="JobLayout">
                    <a class="tab tab-unselect" href="job_active.xml" id="JobFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="JobIconImg" src="tab_job2.gif" alt="작업" border="0" hspace="1px">
                        </span>
                        작업
                    </a>
                </td>
                <td class="tab tab-box-unselect" id="BoxLayout">
                    <a class="tab tab-unselect" href="box_login.xml" id="FileFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="FileIconImg" src="tab_box2.gif" alt="박스" border="0" hspace="1px">
                        </span>
                        박스
                    </a>
                </td>
                <td class="tab tab-print-unselect" id="PrintLayout">
                    <a class="tab tab-unselect" href="print.xml" id="PrintFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="PrintIconImg" src="tab_print2.gif" alt="다이렉트 인쇄" border="0" hspace="1px">
                        </span>
                        <span>다이렉트 인쇄</span>
                    </a>
                </td>
                <td class="tab tab-abbr-unselect" id="AbbrLayout">
                    <a class="tab tab-unselect" href="abbr.xml" id="ScanFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="ScanIconImg" src="tab_abbr2.gif" alt="수신지 등록" border="0" hspace="1px">
                        </span>
                        <span>수신지 등록</span>
                    </a>
                </td>
                <td class="tab tab-fav-unselect" id="FavLayout">
                    <a class="tab tab-unselect" href="favorite.xml" id="FavFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="FavIconImg" src="tab_favorite2.gif" alt="즐겨찾기 설정" border="0" hspace="1px">
                        </span>
                        <span>즐겨찾기 설정</span>
                    </a>
                </td>
                <td></td>
                <td class="tab-top-custom tab-custom-unselect" id="CustomLayout">
                    <a class="tab tab-unselect" href="custom.xml" id="CustomFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.setCookie('');html_f.btnDisabled();">
                        <span class="tab-icon">
                            <img id="CustomIconImg" src="tab_custom2.gif" alt="사용자 설정" border="0" hspace="1px">
                        </span>
                    </a>
                </td>
                <td class="tab-top-menu tab-topmenu-unselect" height="45px">
                    <a class="tab tab-unselect" href="top_menu.xml" id="TopMenuFunction" target="_self" onclick="html_f.closeDialogAll(); html_f.btnDisabled();">
                        <img id="CustomIconImg" src="Return.gif" border="0">
                    </a>
                </td>
            </tr>
        </table>
        <div id="Tab-Border" style="display:none">
            <div id="DummyBorder" class="tab-border-dummy" style="display:block"></div>
            <div id="SystemFunctionBorder" class="tab-border tab-border-system" style="display:none"></div>
            <div id="JobFunctionBorder" class="tab-border tab-border-job" style="display:none"></div>
            <div id="FileFunctionBorder" class="tab-border tab-border-box" style="display:none"></div>
            <div id="PrintFunctionBorder" class="tab-border tab-border-print" style="display:none"></div>
            <div id="ScanFunctionBorder" class="tab-border tab-border-abbr" style="display:none"></div>
            <div id="FavFunctionBorder" class="tab-border tab-border-fav" style="display:none"></div>
            <div id="CustomFunctionBorder" class="tab-border tab-border-custom" style="display:none"></div>
            <div id="TopMenuFunctionBorder" class="tab-border tab-border-custom" style="display:none"></div>
            <a href="#MenuStartLink" name="MenuStartLink">
                <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="부 메뉴">
            </a>
        </div>
        <div class="main-layout">
            <div id="SS3" class="menu-layout" style="display:none">
                <a href="#ContentsStartLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차로 링크">
                </a>
                <div class="menu-box">
                    <div class="menu-main" id="M_BoxLogin">
                        <a id="BoxLogin" class="menu menu-unselect" href="javascript:html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_UOU');location.href='box_login.xml';" style="display:block">
                            <img class="menu-img" src="head_fo2.gif" id="BoxLoginImg">
                            <div class="menu-text">박스 열기</div>
                        </a>
                        <button id="BoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_UOU');"></button>
                    </div>
                    <div class="menu-main" id="M_BoxList">
                        <a id="BoxList" class="menu menu-unselect" href="javascript:html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_ULU');location.href='box_list.xml';" style="display:block">
                            <img class="menu-img" src="head_fo2.gif" id="BoxListImg">
                            <div class="menu-text">박스 목록</div>
                        </a>
                        <button id="BoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_ULU');"></button>
                    </div>
                    <div class="menu-main" id="M_SysBoxLogin">
                        <a id="SysBoxLogin" class="menu menu-unselect" href="javascript:html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SOU');location.href='box_slogin.xml';" style="display:block">
                            <img class="menu-img" src="head_fo2.gif" id="SysBoxLoginImg">
                            <div class="menu-text">시스템 박스 열기</div>
                        </a>
                        <button id="SysBoxLogin_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SOU');"></button>
                    </div>
                    <div class="menu-main" id="M_SysBoxList">
                        <a id="SysBoxList" class="menu menu-unselect" href="javascript:html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SLU');location.href='box_slist.xml';" style="display:block">
                            <img class="menu-img" src="head_fo2.gif" id="SysBoxListImg">
                            <div class="menu-text">시스템 박스 목록</div>
                        </a>
                        <button id="SysBoxList_Button" style="display:none" onclick="html_f.btnDisabled();html_f.closeDialogAll();html_f.setCookie('F_SLU');"></button>
                    </div>
                    <hr class="menu-end" title="메뉴 종료">
                </div>
                <a name="ContentsStartLink" href="#ContentsStartLink">
                    <img class="hidden-link-image" width="1" height="1" src="skip.gif" alt="목차">
                </a>
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
                                    박스 목록<img src="arrow.gif" id="WS_MC_BoxList" alt=" " class="move-contents" onclick="moveToFavorite('MC_BoxList');" onmouseover="ContentsDragDropRegist('MC_BoxList');" onmouseout="ContentsDragDropDestroy('MC_BoxList');">
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
                                    <input type="hidden" name="h_token" value="PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9">
                                    <input type="hidden" id="H_SE" name="H_SER" value="">
                                    <table class="data-table-noline box-data-table-noline" width="505px">
                                        <tr>
                                            <td width="180px"></td>
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
                                            <td width="180px"></td>
                                            <th scope="row" width="150px" valign="top">검색문자로 검색</th>
                                            <td width="80px">
                                                <input type="hidden" name="func" value="PSL_F_ULU_PAG">
                                                <input type="hidden" id="H_PA" name="H_PAG" value="">
                                                <input type="hidden" name="h_token" value="PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9">
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
                                                <label style="margin-left:20px;">줍는아저씨</label>
                                            </td>
                                            <td width="100px">
                                                공유<br>
                                            </td>
                                            <td width="110px">공유</td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="hidden" id="1_PAF_NotRelayAuthOn" value="false">
                                                    <input type="hidden" id="1_BID" value="1">
                                                    <input type="hidden" id="1_BNM" value="줍는아저씨">
                                                    <input type="button" value="편집" id="F_ULU_Bset" name="F_ULU_Bset" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','1','true','_NotRelayAuthOn', 'Setting');">
                                                </div>
                                            </td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="button" value="삭제" id="F_ULU_Bdel" name="F_ULU_Bdel" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','1','true','_NotRelayAuthOn', 'Delete');">
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="75px">
                                                2<br>
                                            </td>
                                            <td width="110px">
                                                <label style="margin-left:20px;">민규</label>
                                            </td>
                                            <td width="100px">
                                                공유<br>
                                            </td>
                                            <td width="110px">공유</td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="hidden" id="2_PAF_NotRelayAuthOn" value="false">
                                                    <input type="hidden" id="2_BID" value="2">
                                                    <input type="hidden" id="2_BNM" value="민규">
                                                    <input type="button" value="편집" id="F_ULU_Bset" name="F_ULU_Bset" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','2','true','_NotRelayAuthOn', 'Setting');">
                                                </div>
                                            </td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="button" value="삭제" id="F_ULU_Bdel" name="F_ULU_Bdel" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','2','true','_NotRelayAuthOn', 'Delete');">
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="75px">
                                                3<br>
                                            </td>
                                            <td width="110px">
                                                <label style="margin-left:20px;">김성진</label>
                                            </td>
                                            <td width="100px">
                                                공유<br>
                                            </td>
                                            <td width="110px">공유</td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="hidden" id="3_PAF_NotRelayAuthOn" value="false">
                                                    <input type="hidden" id="3_BID" value="3">
                                                    <input type="hidden" id="3_BNM" value="김성진">
                                                    <input type="button" value="편집" id="F_ULU_Bset" name="F_ULU_Bset" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','3','true','_NotRelayAuthOn', 'Setting');">
                                                </div>
                                            </td>
                                            <td width="50px" align="center" class="btn-edit-del">
                                                <div class="div-btn-edit-del">
                                                    <input type="button" value="삭제" id="F_ULU_Bdel" name="F_ULU_Bdel" class="btn" onclick="box_f.handleFav();box_f.setCookie('F_ULU');box_f.execBoxLogin('F_ULU','User','3','true','_NotRelayAuthOn', 'Delete');">
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                                <form method="POST" name="F_ULU_PTR" id="F_ULUtrueUser_PTR" onsubmit="return box_f.ChkList('F_ULUtrueUser_');" action="user.cgi" Accept-charset="UTF-8">
                                    <input type="hidden" name="H_TAB" id="H_F_BOXLIST_PTR" value="">
                                    <input type="hidden" name="func" value="PSL_F_ULUUser_BOX">
                                    <input type="hidden" name="h_token" value="PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9">
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
                            <input type="hidden" name="h_token" value="PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9">
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
        <input type="hidden" name="h_token" value="PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9">
        <h1 class="contents-header">로그아웃</h1>
        <div class="contents-body">
            <p>공유 사용자로부터 로그아웃 하고, 로그인 화면으로 이동하시겠습니까?</p>
        </div>
        <hr class="contents-end" title="종료">
        <div class="buttonarea-layout">
            <input type="submit" value="확인" class="button-height">
            <input type="button" value="취소" onclick="html_f.ComReturnDisplay()" class="button-height">
        </div>
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
http://192.168.11.112/wcd/user.cgi
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.112:80
Referrer Policy
strict-origin-when-cross-origin
content-length
755
content-type
text/xml
expires
Thu, 01 Jan 1970 00:00:00 GMT
x-frame-options
SAMEORIGIN
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
cache-control
max-age=0
connection
keep-alive
content-length
176
content-type
application/x-www-form-urlencoded
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; param=; access=; bm=Low; abbrRedojobingStatus=allow; selno=Ko; wd=n; help=off,off,off; adm=AS_COU; ver_expires=Tue, 13 Apr 2027 10:48:25 GMT; ID=AVtNYoFbvQjXgyUJnvYK5Oyhfa98zyuJ; abbrCheckCookieFlg=true; loginUserName=Public; usr=F_ULU
host
192.168.11.112
origin
http://192.168.11.112
referer
http://192.168.11.112/wcd/box_create.xml
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: func=PSL_F_ULUUser_CRE&h_token=PYn9GOOQH2hyPRkp4ieBaKi5cxodttN9&H_TAB=&R_NUM=Space&T_NAM=TEST_NAME&C_USE=UsePass&P_PAS=1234&P_PAS2=1234&S_SER=Abc&R_SAP=None&S_GFC=Off&S_SEC=Off
