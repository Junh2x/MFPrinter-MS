## 문서서버 폴더 리스트 조회
Request URL
http://192.168.11.228/web/guest/ko/webdocbox/folderListPage.cgi
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.228:80
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache
connection
Keep-Alive
content-type
text/html; charset=UTF-8
date
Mon, 13 Apr 2026 02:58:00 GMT
expires
Mon, 13 Apr 2026 02:58:00 GMT
pragma
no-cache
server
Web-Server/3.0
set-cookie
cookieOnOffChecker=on; path=/
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
cookie
risessionid=095583375732754; cookieOnOffChecker=on; wimsesid=547625615
host
192.168.11.228
referer
http://192.168.11.228/web/guest/ko/websys/webArch/topPage.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
응답:
<html lang="ko">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <meta http-equiv="Content-Script-Type" content="text/javascript">
        <meta http-equiv="Content-Style-Type" content="text/css">
        <meta http-equiv="Cache-Control" content="no-cache">
        <meta http-equiv="Pragma" content="no-cache">
        <meta http-equiv="Expires" content="-1">
        <script language="JavaScript" src="webdocbox/DocumentBoxCommon.xjs"></script>
        <script language="JavaScript" src="webdocbox/webdocboxOnMouse.xjs"></script>
        <script language="JavaScript" src="webdocbox/wuaCommon.xjs"></script>
        <script language="JavaScript" src="webdocbox/folderListCommon.xjs"></script>
        <script language="JavaScript" src="/scripts/arrowimg.js"></script>
        <script language="JavaScript" src="/scripts/base64.js"></script>
        <script language="JavaScript" src="/scripts/common.js"></script>
        <script language="JavaScript" src="/scripts/reload.js"></script>
        <script language="JavaScript" src="/scripts/sortimg.js"></script>
        <script language="JavaScript" src="/scripts/isNumericInput.js"></script>
        <link href="/css/common.css" type="text/css" rel="stylesheet">
        <title>문서 서버</title>
        <style type="text/css"></style>
        <style type="text/css">
            table.defaultTableButton {
                font-size: 12px;
                border-style: solid;
                border-width: 1px;
                border-color: #8D8D8D;
                cursor: pointer;
            }

            table.defaultTableButton td.defaultTableButton {
                padding: 0px 8px 0px 8px;
                vertical-align: middle;
                background-color: #808080;
                background-image: URL(/images/buttonBGwhite.gif);
                background-repeat: repeat-x;
                text-align: center;
                overflow: hidden;
            }

            table.defaultTableButtonSelected {
                font-size: 12px;
                border-style: solid;
                border-width: 1px;
                border-color: #8D8D8D;
                cursor: pointer;
            }

            table.defaultTableButtonSelected td.defaultTableButtonSelected {
                padding: 0px 8px 0px 8px;
                vertical-align: middle;
                background-color: #F7EEB2;
                text-align: center;
                overflow: hidden;
            }

            table.defaultTableButton a.defaultTableButton {
                text-decoration: none;
                color: black;
                white-space: nowrap;
            }

            table.defaultTableButtonSelected a.defaultTableButton {
                text-decoration: none;
                color: black;
                white-space: nowrap;
            }

            table.defaultTableButton a.defaultTableCommandButton {
                text-decoration: none;
                font-weight: bold;
                color: black;
                white-space: nowrap;
            }

            table.defaultTableButtonSelected a.defaultTableCommandButton {
                text-decoration: none;
                font-weight: bold;
                color: black;
                white-space: nowrap;
            }
        </style>
        <script language="Javascript" type="text/javascript">
            function sortBtnMouseOver(obj) {
                var sortImage = obj.parentNode.getElementsByTagName("a")[1].getElementsByTagName("img")[0];
                var imgPath = sortImage.getAttribute("src");
                if (imgPath.indexOf("-s.gif") > 0) {
                    return;
                }
                imgPath = imgPath.replace(".gif", "-r.gif");
                sortImage.setAttribute("src", imgPath);
            }
            function sortBtnMouseOut(obj) {
                var sortImage = obj.parentNode.getElementsByTagName("a")[1].getElementsByTagName("img")[0];
                var imgPath = sortImage.getAttribute("src");
                imgPath = imgPath.replace("-r.gif", ".gif");
                if (imgPath.indexOf("-s.gif") > 0) {
                    return;
                }
                sortImage.setAttribute("src", imgPath);
            }
        </script>
    </head>
    <body bgcolor="#cccccc" text="#000000" link="#000000" alink="#000000" vlink="#000000" marginwidth="0" leftmargin="0" marginheight="0" topmargin="0">
        <table width="100%" height="30" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td></td>
                <td>
                    <img src="/images/spacer.gif" width="1" height="4" border="0" alt="" title="">
                </td>
                <td></td>
            </tr>
            <tr>
                <td align="left" valign="top" width="12">
                    <img width="12" border="0" src="/images/spacer.gif" alt="" title="">
                </td>
                <td nowrap width="100%" align="left" height="30">
                    <table border="0" cellspacing="0" height="30" width="100%">
                        <tr>
                            <td nowrap align="left" valign="middle" width="30%">
                                <div style="color:black; font-size:16px; font-weight:bold;">
                                    <img src="/images/spacer.gif" width="8" height="1" border="0" alt="" title="">문서 서버
                                </div>
                            </td>
                            <td></td>
                            <td nowrap align="right" valign="middle" width="25">
                                <div class="commandLabel">
                                    <a class="commandLabel" onmouseover="changeImage(reloadButton4Admin,'img9999-reload',0)" onmouseout="changeImage(reloadButton4Admin,'img9999-reload',1)" href="javaScript:folderListFormSubmit(3,document.reportListForm)">
                                        <nobr>
                                            <img border="0" src="/images/btnReload.gif" name="img9999-reload" align="absmiddle" alt="" title="">새로 고침
                                        </nobr>
                                    </a>
                                </div>
                            </td>
                            <form name="help">
                                <input type="hidden" name="ID">
                                <td nowrap align="right" valign="middle" width="25">
                                    <div class="commandLabel">
                                        <a class="commandLabel" onmouseover="changeImage(helpButton,'img9999',0)" onmouseout="changeImage(helpButton,'img9999',1)" href="javascript:toHelp('..','box10004.html')">
                                            <nobr>
                                                <img border="0" src="/images/helpBtnSetting.gif" name="img9999" align="absmiddle" alt="도움말" title="도움말">
                                            </nobr>
                                        </a>
                                    </div>
                                </td>
                            </form>
                            <td width="5"></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <table width="100%" border="0" cellspacing="0" cellpadding="0" class="settingFlatContsDivision">
                        <tr>
                            <td>
                                <img src="/images/settingFlatContsDivision.gif" alt="" title="">
                            </td>
                        </tr>
                    </table>
                </td>
                <td></td>
            </tr>
            <tr>
                <td height="15px"></td>
            </tr>
        </table>
        <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
            <tr class="standard">
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="32" border="0">
                </td>
                <td nowrap align="left" valign="top">
                    <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td nowrap>
                                <table border="0" cellspacing="0" cellpadding="0">
                                    <tr class="staticProp">
                                        <td>
                                            <table border="0" cellspacing="0" cellpadding="0">
                                                <tr>
                                                    <td>
                                                        <table width="100%" class="defaultTableButton" cellspacing="0">
                                                            <tr>
                                                                <td class="defaultTableButton" onclick="javaScript:goHome(); return false;" height="20">
                                                                    <a href="javaScript:goHome()" class="defaultTableCommandButton">뒤로</a>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td nowrap width="1" height="1">
                                                        <img src="/images/spacer.gif" height="1" width="100" border="0" alt="" title="">
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="settingDivision">
                                    <tr>
                                        <td>
                                            <img src="/images/settingDivision.gif" alt="" title="">
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
            <tr class="standard">
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="32" border="0">
                </td>
                <td nowrap align="left" valign="top">
                    <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td nowrap align="left" valign="bottom">
                                <table height="24" border="0" cellspacing="0" cellpadding="0">
                                    <tr>
                                        <td nowrap align="left" valign="middle">
                                            <div class="commandLabel">
                                                <a class="commandLabel" onmouseover="changeImage(buttonImg,'img0',1)" onmouseout="changeImage(buttonImg,'img0',0)" href="javascript:folderList_add();">
                                                    <nobr>
                                                        <img border="0" src="/images/cmdSendFolder.gif" name="img0" align="absmiddle" alt="" title="">새 폴더
                                                    </nobr>
                                                </a>
                                            </div>
                                        </td>
                                        <td width="12"></td>
                                        <td nowrap align="left" valign="middle">
                                            <div class="commandLabel">
                                                <a class="commandLabel" onmouseover="changeImage(buttonImg,'img2',3)" onmouseout="changeImage(buttonImg,'img2',2)" href="javascript:folderList_edit();">
                                                    <nobr>
                                                        <img border="0" src="/images/cmdChange.gif" name="img2" align="absmiddle" alt="" title="">폴더 편집
                                                    </nobr>
                                                </a>
                                            </div>
                                        </td>
                                        <td width="12"></td>
                                        <td nowrap align="left" valign="middle">
                                            <div class="commandLabel">
                                                <a class="commandLabel" onmouseover="changeImage(buttonImg,'img18',19)" onmouseout="changeImage(buttonImg,'img18',18)" href="javascript:folderList_delete();">
                                                    <nobr>
                                                        <img border="0" src="/images/cmdDelete.gif" name="img18" align="absmiddle" alt="" title="">폴더 삭제
                                                    </nobr>
                                                </a>
                                            </div>
                                        </td>
                                        <td width="12"></td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
                <td nowrap align="right">
                    <table border="0">
                        <tr class="staticProp">
                            <td nowrap style="padding-left:20px;">
                                <a href="javaScript:location.href='../websys/webArch/jobList.cgi';">
                                    [작업]으로 이동<font style="font-family:osaka, verdana, arial, helvetica, sans-serif, 'MS UI Gothic';">&gt;&gt;</font>
                                </a>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <table width="100%" height="*" border="0" cellspacing="0" cellpadding="0">
            <tr class="standard">
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="100%" border="0">
                </td>
                <td nowrap align="left" valign="top">
                    <form name="filterForm" onSubmit="return false;" method="post" target="_self" style="margin-bottom:6px;">
                        <input type="hidden" name="wimToken" value="462279011">
                        <input type="hidden" name="filter_propName" value="">
                        <input type="hidden" name="filter_propValue" value="">
                        <input type="hidden" name="offset" value="0">
                        <input type="hidden" name="total" value="3" disabled>
                        <input type="hidden" name="resultRowBlockSize" value="">
                        <input type="hidden" name="orderBy_property" value="" disabled>
                        <input type="hidden" name="orderBy_descendingRequested" value="" disabled>
                        <input type="hidden" name="useInputParam" value="">
                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                            <tr>
                                <td>
                                    <table border="0" cellspacing="0" cellpadding="0">
                                        <tr>
                                            <td nowrap height="8"></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table border="0" cellspacing="0" cellpadding="0">
                                        <tr class="standard">
                                            <td nowrap></td>
                                            <td nowrap align="left" style="padding-left:12px;">폴더 이름별로 검색</td>
                                            <td nowrap>:</td>
                                            <td nowrap width="8" align="left"></td>
                                            <td nowrap align="left" valign="middle">
                                                <input type="hidden" name="filterName" value="FOLDER_NAME">
                                                <input type="text" name="filterValue" size="28" maxlength="64" value="">
                                            </td>
                                            <td nowrap width="8" align="left"></td>
                                            <td nowrap align="left" valign="middle">
                                                <input type="button" name="search04" onClick="folderListChangeFilter(1)" value="검색">
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table border="0" cellspacing="0" cellpadding="0">
                                        <tr>
                                            <td nowrap height="8"></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table border="0" cellspacing="0" cellpadding="0">
                                        <tr class="standard">
                                            <td>
                                                <table class="standard" cellspacing="0" cellpadding="0">
                                                    <tr height="28">
                                                        <td nowrap align="left" valign="middle">
                                                            <div class="commandLabel">
                                                                <nobr>
                                                                    <img border="0" src="/images/btnArrowLeft2-d.gif" name="imgTop" align="absmiddle" alt="" title="">
                                                                </nobr>
                                                            </div>
                                                        </td>
                                                        <td width="10"></td>
                                                        <td nowrap align="left" valign="middle">
                                                            <div class="commandLabel">
                                                                <nobr>
                                                                    <img border="0" src="/images/btnArrowLeft1-d.gif" name="imgPrevious" align="absmiddle" alt="" title="">
                                                                </nobr>
                                                            </div>
                                                        </td>
                                                        <td width="12"></td>
                                                        <td>1/1</td>
                                                        <td width="12"></td>
                                                        <td nowrap align="left" valign="middle">
                                                            <div class="commandLabel">
                                                                <nobr>
                                                                    <img border="0" src="/images/btnArrowRight1-d.gif" name="imgNext" align="absmiddle" alt="" title="">
                                                                </nobr>
                                                            </div>
                                                        </td>
                                                        <td width="10"></td>
                                                        <td nowrap align="left" valign="middle">
                                                            <div class="commandLabel">
                                                                <nobr>
                                                                    <img border="0" src="/images/btnArrowRight2-d.gif" name="imgLast" align="absmiddle" alt="" title="">
                                                                </nobr>
                                                            </div>
                                                        </td>
                                                        <td width="15"></td>
                                                        <td align="right">
                                                            페이지<img src="/images/spacer.gif" width="4" alt="" title="">
                                                            :<img src="/images/spacer.gif" width="4" alt="" title="">
                                                        </td>
                                                        <td width="*" align="right">
                                                            <input type="text" name="pages" size="4" maxlength="3" onkeydown="return isNumericInput(event)" onkeypress="return isNumericInput(event)" onkeyup="return isNumericInput(event)" onblur="return isNumericInput(this)" style="ime-mode:disabled">
                                                        </td>
                                                        <td width="40" align="right">
                                                            <input type="button" name="go04" onclick="folderListChangeFilter(15);" value="이동">
                                                        </td>
                                                        <td width="15"></td>
                                                        <td class="standard">
                                                            표시 항목<img src="/images/spacer.gif" width="4" alt="" title="">
                                                            :<img src="/images/spacer.gif" width="4" alt="" title="">
                                                        </td>
                                                        <td>
                                                            <select name="rowSize" size="1" onchange="folderListChangeFilter(20)">
                                                                <option value="5">5</option>
                                                                <option value="6">6</option>
                                                                <option value="7">7</option>
                                                                <option value="8">8</option>
                                                                <option value="9">9</option>
                                                                <option value="10" selected>10</option>
                                                                <option value="11">11</option>
                                                                <option value="12">12</option>
                                                                <option value="13">13</option>
                                                                <option value="14">14</option>
                                                                <option value="15">15</option>
                                                                <option value="16">16</option>
                                                                <option value="17">17</option>
                                                                <option value="18">18</option>
                                                                <option value="19">19</option>
                                                                <option value="20">20</option>
                                                            </select>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td nowrap height="1" width="12" align="left" valign="top">
                                                <img src="/images/spacer.gif" alt="" width="12" height="1" border="0">
                                            </td>
                                            <td nowrap>등록된 총 항목 수</td>
                                            <td>:</td>
                                            <td nowrap>3</td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </form>
                </td>
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="6" border="0">
                </td>
            </tr>
            <tr class="standard">
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="100%" border="0">
                </td>
                <td>
                    <form name="reportListForm" onSubmit="return false;" method="post" target="_self">
                        <input type="hidden" name="wimToken" value="462279011">
                        <input type="hidden" name="mode" value="">
                        <input type="hidden" name="selectedDocIds" value="">
                        <input type="hidden" name="subReturnDsp" value="">
                        <input type="hidden" name="useInputParam" value="">
                        <input type="hidden" name="useSavedPropParam" value="false">
                        <table class="reportListCommon" cellspacing="0" cellpadding="0" border="0" width="100%" bgcolor="#adadad">
                            <tr valign="top">
                                <td>
                                    <table class="listHeader" border="0" width="100%" cellpadding="0" cellspacing="0">
                                        <tr height="27" bgcolor="#E6E6E6">
                                            <td>
                                                <table>
                                                    <tr height="25">
                                                        <td nowrap>
                                                            <div class="listTitle">
                                                                <font color="#000000"></font>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    <table class="listHeader" border="0" width="100%" cellpadding="0" cellspacing="0">
                                        <tr height="27" bgcolor="#E6E6E6">
                                            <td>
                                                <table>
                                                    <tr height="25">
                                                        <td nowrap>
                                                            <div class="listTitle">
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);" name="FOLDER_NUMBER" class="asc">폴더 번호</a>
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);">
                                                                    <img width="12" height="6" src="/images/btnSortUP-s.gif" border="0" alt="오름차순을 선택했습니다." title="오름차순을 선택했습니다.">
                                                                </a>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    <table class="listHeader" border="0" width="100%" cellpadding="0" cellspacing="0">
                                        <tr height="27" bgcolor="#E6E6E6">
                                            <td>
                                                <table>
                                                    <tr height="25">
                                                        <td nowrap>
                                                            <div class="listTitle">
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);" name="FOLDER_NAME" class="dsc">폴더 이름</a>
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);">
                                                                    <img width="12" height="6" src="/images/btnSortDown.gif" border="0" alt="오름차순" title="오름차순">
                                                                </a>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    <table class="listHeader" border="0" width="100%" cellpadding="0" cellspacing="0">
                                        <tr height="27" bgcolor="#E6E6E6">
                                            <td>
                                                <table>
                                                    <tr height="25">
                                                        <td nowrap>
                                                            <div class="listTitle">
                                                                <font color="#000000">암호</font>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    <table class="listHeader" border="0" width="100%" cellpadding="0" cellspacing="0">
                                        <tr height="27" bgcolor="#E6E6E6">
                                            <td>
                                                <table>
                                                    <tr height="25">
                                                        <td nowrap>
                                                            <div class="listTitle">
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);" name="MakingDay" class="dsc">생성 날짜/시간</a>
                                                                <a href="#r" onMouseOver="javascript:sortBtnMouseOver(this)" onMouseOut="javascript:sortBtnMouseOut(this)" onClick="javascript:folderListSortBtnClick(this);">
                                                                    <img width="12" height="6" src="/images/btnSortDown.gif" border="0" alt="오름차순" title="오름차순">
                                                                </a>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData" align="center"></td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <div align="center">---</div>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <img src="/images/btnSharedFolder.gif" _src="/images/btnFolder-r.gif" alt="" title="" border="0" style="margin-right:5px; vertical-align:middle;">
                                        <a href="docListPage.cgi?selectedFolderId=0&amp;subReturnDsp=3">공유 폴더</a>
                                    </nobr>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <div align="center">---</div>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <span id="_year">----</span>
                                        /<span id="_month">--</span>
                                        /<span id="_day">--</span>
                                        --<input type="hidden" name="_hour" value="">
                                        :--<input type="hidden" name="_min" value="">
                                    </nobr>
                                </td>
                            </tr>
                            <tr>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData" align="center">
                                    <input type="radio" name="selectedFolderId" value="1" style="width:16px; height:16px;">
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">001</td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <img src="/images/btnFolder.gif" _src="/images/btnFolder-r.gif" alt="" title="" border="0" style="margin-right:5px; vertical-align:middle;">
                                        <a href="docListPage.cgi?selectedFolderId=1&amp;subReturnDsp=3">김민규</a>
                                    </nobr>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <div align="center">---</div>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <span id="_year">2026</span>
                                        /<span id="_month">03</span>
                                        /<span id="_day">17</span>
                                        15<input type="hidden" name="_hour" value="15">
                                        :28<input type="hidden" name="_min" value="28">
                                    </nobr>
                                </td>
                            </tr>
                            <tr>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData" align="center">
                                    <input type="radio" name="selectedFolderId" value="2" style="width:16px; height:16px;">
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">002</td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <img src="/images/btnFolder.gif" _src="/images/btnFolder-r.gif" alt="" title="" border="0" style="margin-right:5px; vertical-align:middle;">
                                        <a href="docListPage.cgi?selectedFolderId=2&amp;subReturnDsp=3">김민규2</a>
                                    </nobr>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <div align="center">---</div>
                                </td>
                                <td style="word-break:break-all; padding:3px;" bgcolor="#ffffff" class="listData">
                                    <nobr>
                                        <span id="_year">2026</span>
                                        /<span id="_month">03</span>
                                        /<span id="_day">17</span>
                                        15<input type="hidden" name="_hour" value="15">
                                        :28<input type="hidden" name="_min" value="28">
                                    </nobr>
                                </td>
                            </tr>
                        </table>
                    </form>
                </td>
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="100%" border="0">
                </td>
                <td nowrap>
                    <form name="popupText">
                        <input type="hidden" name="wimToken" value="462279011">
                        <input type="hidden" name="nonselectAlertText" value="폴더를 선택하지 않았습니다.">
                        <input type="hidden" name="deleteConfirmText" value="폴더와 폴더 내 파일을 모두 삭제하시겠습니까?">
                        <input type="hidden" name="unlockConfirmText" value="폴더의 잠금을 해제하시겠습니까?">
                    </form>
                </td>
            </tr>
        </table>
        <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
            <tr class="standard">
                <td nowrap width="12" align="left" valign="top">
                    <img src="/images/spacer.gif" alt="" title="" width="12" height="32" border="0">
                </td>
                <td nowrap align="left" valign="top">
                    <table width="100%" height="32" border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td nowrap>
                                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="settingDivision">
                                    <tr>
                                        <td>
                                            <img src="/images/settingDivision.gif" alt="" title="">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellspacing="0" cellpadding="0">
                                    <tr class="staticProp">
                                        <td>
                                            <table border="0" cellspacing="0" cellpadding="0">
                                                <tr>
                                                    <td>
                                                        <table width="100%" class="defaultTableButton" cellspacing="0">
                                                            <tr>
                                                                <td class="defaultTableButton" onclick="javaScript:goHome(); return false;" height="20">
                                                                    <a href="javaScript:goHome()" class="defaultTableCommandButton">뒤로</a>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td nowrap width="1" height="1">
                                                        <img src="/images/spacer.gif" height="1" width="100" border="0" alt="" title="">
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
</html>

## 폴더 내 파일 목록 조회
Request URL
http://192.168.11.228/web/guest/ko/webdocbox/docListPage.cgi?selectedFolderId=2&subReturnDsp=3
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.228:80
Referrer Policy
strict-origin-when-cross-origin
cache-control
no-cache
connection
Keep-Alive
content-type
text/html; charset=UTF-8
date
Mon, 13 Apr 2026 02:59:12 GMT
expires
Mon, 13 Apr 2026 02:59:12 GMT
pragma
no-cache
server
Web-Server/3.0
set-cookie
cookieOnOffChecker=on; path=/
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
cookie
risessionid=095583375732754; cookieOnOffChecker=on; wimsesid=547625615
host
192.168.11.228
referer
http://192.168.11.228/web/guest/ko/webdocbox/folderListPage.cgi
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: selectedFolderId=2&subReturnDsp=3
응답:
<html lang="ko">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <title>김민규2</title>
        <meta http-equiv="Content-Script-Type" content="text/javascript">
        <meta http-equiv="Cache-Control" content="no-cache">
        <meta http-equiv="Pragma" content="no-cache">
        <meta http-equiv="Expires" content="-1">
        <link href="/css/common.css" type="text/css" rel="stylesheet">
        <link href="/css/cmnParts.css" type="text/css" rel="stylesheet">
        <script language="JavaScript" src="/scripts/jquery.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/jquery.ui.core.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/jquery.ui.widget.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/jquery.ui.mouse.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/jquery.ui.draggable.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/jquery.ui.droppable.1.4.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/common.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/reload.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/ajax.js" type="text/javascript"></script>
        <script language="JavaScript" type="text/javascript">
            function jumpTo(url) {
                location.href = url;
            }
        </script>
        <style type='text/css'>
            .borderless td {
                border-style: none;
            }

            .docListData {
                word-break: break-all;
                padding: 3px
            }

            input.passButton {
                font-weight: normal;
                padding: 0 2px;
            }
        </style>
        <script language="JavaScript" src="/scripts/arrowimg.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/base64.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/sortimg.js" type="text/javascript"></script>
        <script language="JavaScript" src="/scripts/isNumericInput.js" type="text/javascript"></script>
        <script language="JavaScript" src="webdocbox/DocumentBoxCommon.xjs" type="text/javascript"></script>
        <script language="JavaScript" src="webdocbox/webdocboxOnMouse.xjs" type="text/javascript"></script>
        <script language="JavaScript" src="webdocbox/wuaCommon.xjs" type="text/javascript"></script>
        <script language="JavaScript" language="JavaScript">
            function errorDisp(html) {
                document.getElementsByTagName("body")[0].innerHTML = html;
            }
            function fileDownload(url) {
                getDlurl_b64toDec(url, document.buttonform);
                document.buttonform.target = "_self";
                document.buttonform.method = "post";
                document.buttonform.submit();
            }
        </script>
    </head>
    <body onload="javaScript:onLoadSettingListForm(document.DocboxListForm);initImg();">
        <a name="TOP" id="TOP"></a>
        <div id="wrap">
            <div id="frame_top">
                <div id="h4tab">
                    <div id="h4">
                        <h4>김민규2</h4>
                    </div>
                    <ul>
                        <li>
                            <a class="commandLabel" onmouseover="changeImage(reloadButton4Admin,'img9999-reload',0)" onmouseout="changeImage(reloadButton4Admin,'img9999-reload',1)" href="javaScript:ButtonFormSubmit(3)">
                                <nobr>
                                    <img border="0" src="/images/btnReload.gif" name="img9999-reload" height="20" class="ver-algn-m">새로 고침
                                </nobr>
                            </a>
                        </li>
                        <li>
                            <a class="commandLabel" onmouseover="changeImage(helpButton,'img9999',0)" onmouseout="changeImage(helpButton,'img9999',1)" href="javascript:toHelp('..','box1010.html')">
                                <nobr>
                                    <img border="0" src="/images/helpBtnSetting.gif" name="img9999" alt="도움말" title="도움말" height="20" class="ver-algn-m">
                                </nobr>
                            </a>
                        </li>
                    </ul>
                </div>
                <div id='button'>
                    <ul>
                        <li>
                            <input type="button" value="뒤로" onClick="javaScript:folderListFormSubmit(1,document.DocboxListForm);" class='defaultButton'></input>
                        </li>
                    </ul>
                </div>
                <p class="clear Bodder">
                    <img src="/images/settingDivision.gif" alt="" title="">
                </p>
            </div>
            <form name="buttonform" target="_self" onSubmit="return false;">
                <input type="hidden" name="wimToken" value="462279011">
                <input type="hidden" name="offset" value="0">
                <input type="hidden" name="resultRowBlockSize" value="10">
                <input type="hidden" name="matrixColSpan" value="4">
                <input type="hidden" name="urlLang" value="ko">
                <input type="hidden" name="urlProfile" value="guest">
                <input type="hidden" name="show" value="thumbnail">
                <input type="hidden" name="dummy" value="">
                <input type="hidden" name="applicationType" value="all">
                <input type="hidden" name="filter_propName" value="">
                <input type="hidden" name="filter_propValue" value="">
                <input type="hidden" name="ThumbnailPropAttr" value="password">
                <input type="hidden" name="id" value="">
                <input type="hidden" name="jt" value="">
                <input type="hidden" name="el" value="">
                <input type="hidden" name="orderBy_property" value="creationDate">
                <input type="hidden" name="orderBy_descendingRequested" value="true">
            </form>
            <div class="btnarea">
                <ul>
                    <li>
                        <a href="javaScript:ButtonFormSubmit(1)" onmouseout="changeImage(buttonImg,'img1',24)" onmouseover="changeImage(buttonImg,'img1',25)" class="commandLabel">
                            <img src="/images/cmdPrint.gif" alt="" title="" name="img1" align="absmiddle">인쇄
                        </a>
                    </li>
                    <li>
                        <a href="javaScript:ButtonFormSubmit(9)" onmouseout="changeImage(buttonImg,'img3',14)" onmouseover="changeImage(buttonImg,'img3',15)" class="commandLabel">
                            <img src="/images/cmdSend.gif" alt="" title="" name="img3" align="absmiddle">전송
                        </a>
                    </li>
                    <li>
                        <a href="javaScript:ButtonFormSubmit(2)" onmouseout="changeImage(buttonImg,'img2',18)" onmouseover="changeImage(buttonImg,'img2',19)" class="commandLabel">
                            <img src="/images/cmdDelete.gif" alt="" title="" name="img2" align="absmiddle">삭제
                        </a>
                    </li>
                </ul>
            </div>
            <div class="float-r">
                <p>
                    <a class="commandLabel" href="javaScript:goJob(document.DocboxListForm)">
                        [작업]으로 이동<font style="font-family:osaka, verdana, arial, helvetica, sans-serif, 'MS UI Gothic';">&gt;&gt;</font>
                    </a>
                </p>
            </div>
            <form name="DocboxListForm" onSubmit="return false;">
                <table class="clear mgn-T8px">
                    <tr>
                        <td>보기 &#160;:&#160;</td>
                        <td>
                            <select size="1" name="wkAppliType" onChange="SetSearchInformation(1,document.DocboxListForm,document.DocboxListForm.wkAppliType,document.DocboxListForm.wkFilterName)">
                                <option value="all" selected>모두</option>
                                <option value="docBoxPrint">인쇄 가능 파일</option>
                                <option value="copy">복사기</option>
                                <option value="print">프린터</option>
                                <option value="fax">팩스</option>
                                <option value="scan">스캐너</option>
                            </select>
                        </td>
                        <td style="padding-left:42px;">검색 &#160;:&#160;</td>
                        <td>
                            <select size="1" name="wkFilterName">
                                <option value="title">파일 이름</option>
                                <option value="creator">사용자 이름</option>
                            </select>
                        </td>
                        <td>&#160;:&#160;</td>
                        <td>
                            <input type="text" name="wkFilterValue" value="" size="16" maxLength="64">
                        </td>
                        <td style="padding-left:16px;">
                            <input type="button" value="검색" onclick="javascript:SetSearchInformation(2,document.DocboxListForm,document.DocboxListForm.wkAppliType,document.DocboxListForm.wkFilterName)">
                        </td>
                    </tr>
                </table>
                <table class='clear mgn-T8px'>
                    <tr>
                        <td>
                            <img src="/images/btnArrowLeft2-d.gif" alt="" title="" name="imgTop" align="absmiddle">
                        </td>
                        <td style='padding-left:10px;'>
                            <img src="/images/btnArrowLeft1-d.gif" alt="" title="" name="imgPrevious" align="absmiddle">
                        </td>
                        <td style='padding-left:12px;'>1/1</td>
                        <td style='padding-left:12px;'>
                            <img src="/images/btnArrowRight1-d.gif" alt="" title="" name="imgNext" align="absmiddle">
                        </td>
                        <td style='padding-left:10px;'>
                            <img src="/images/btnArrowRight2-d.gif" alt="" title="" name="imgLast" align="absmiddle">
                        </td>
                        <td style='padding-left:15px;'>페이지 &#160;:&#160;</td>
                        <td>
                            <input type="text" name="pages" value="" size="4" maxLength="3" onkeydown="return isNumericInput(event)" onkeypress="return isNumericInput(event)" onkeyup="return isNumericInput(event)" onblur="return isNumericInput(this)" style='ime-mode:disabled'>
                            <input type="button" value="GO" onclick="javascript:PagesControl(document.DocboxListForm)">
                        </td>
                        <td style='padding-left:15px;'>표시 항목 &#160;:&#160;</td>
                        <td>
                            <select name='wkBlockSize' size='1' onchange='BlockSizeChenge(document.DocboxListForm,document.DocboxListForm.wkBlockSize)'>
                                <option value='5'>5</option>
                                <option value='6'>6</option>
                                <option value='7'>7</option>
                                <option value='8'>8</option>
                                <option value='9'>9</option>
                                <option value='10' selected>10</option>
                                <option value='11'>11</option>
                                <option value='12'>12</option>
                                <option value='13'>13</option>
                                <option value='14'>14</option>
                                <option value='15'>15</option>
                                <option value='16'>16</option>
                                <option value='17'>17</option>
                                <option value='18'>18</option>
                                <option value='19'>19</option>
                                <option value='20'>20</option>
                            </select>
                        </td>
                        <td style='padding-left:100px;'>
                            <img src="/images/btnThumbnail-r.gif" alt="섬네일이 표시됩니다." title="섬네일이 표시됩니다." name="" align="absmiddle">
                            <a href="javaScript:ChengeShow(2,document.DocboxListForm)" onmouseout="changeImage(buttonImg,'imgshow2',10)" onmouseover="changeImage(buttonImg,'imgshow2',11)" class="commandLabel">
                                <img src="/images/btnIcon.gif" alt="아이콘 표시" title="아이콘 표시" name="imgshow2" align="absmiddle">
                            </a>
                            <a href="javaScript:ChengeShow(3,document.DocboxListForm)" onmouseout="changeImage(buttonImg,'imgshow3',12)" onmouseover="changeImage(buttonImg,'imgshow3',13)" class="commandLabel">
                                <img src="/images/btnList.gif" alt="세부 정보 표시" title="세부 정보 표시" name="imgshow3" align="absmiddle">
                            </a>
                        </td>
                    </tr>
                </table>
                <div class="mgn-T8px clear">
                    <table width="98%" cellspacing="0" cellpadding="0" bgcolor="#FFFFFF" style="border:1px #AFAFAF solid;border-collapse:collapse;">
                        <tr>
                            <td nowrap height="24" align="left" valign="middle" border="0">
                                <table class="borderless">
                                    <tr align="center">
                                        <td style="padding-left:10px;">총 파일 수 &#160;:&#160;0</td>
                                        <td style="padding-left:10px;">선택한 파일 &#160;:&#160;</td>
                                        <td style="border-style:none;">
                                            <input type="text" name="selectCount" value="0" size="2" maxLength="5" readonly style="background-color:#FFFFFF; border-width:0; font-size:12;">
                                        </td>
                                        <td style="padding-left:10px;">
                                            <input type="button" value="모두 선택 해제" onclick="javascript:SetAllSelectionCancel(document.DocboxListForm)" disabled="" name="AllSelectionCancel">
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td valign="middle" nowrap>
                                <div align="right">
                                    <table border="0" cellspacing="0" cellpadding="0">
                                        <tr height="6">
                                            <td class='commandLabel' style='padding-left:16px;' nowrap>
                                                <span style="margin-right:4px;">파일 이름</span>
                                                <span>
                                                    <a href="javascript:SetAscendingData('title',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnDocumentNameup',1)" onmouseover="changeImage(sortImg,'imgcolumnDocumentNameup',2)" class="commandLabel">
                                                        <img src="/images/btnSortUP.gif" alt="오름차순" title="오름차순" name="imgcolumnDocumentNameup" align="absmiddle">
                                                    </a>
                                                </span>
                                                <span style="margin-left:4px;">
                                                    <a href="javascript:SetDescendingData('title',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnDocumentNamedown',4)" onmouseover="changeImage(sortImg,'imgcolumnDocumentNamedown',5)" class="commandLabel">
                                                        <img src="/images/btnSortDown.gif" alt="내림차순" title="내림차순" name="imgcolumnDocumentNamedown" align="absmiddle">
                                                    </a>
                                                </span>
                                            </td>
                                            <td class='commandLabel' style='padding-left:16px;' nowrap>
                                                <span style="margin-right:4px;">사용자 이름</span>
                                                <span>
                                                    <a href="javascript:SetAscendingData('creator',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnOwnerNameup',1)" onmouseover="changeImage(sortImg,'imgcolumnOwnerNameup',2)" class="commandLabel">
                                                        <img src="/images/btnSortUP.gif" alt="오름차순" title="오름차순" name="imgcolumnOwnerNameup" align="absmiddle">
                                                    </a>
                                                </span>
                                                <span style="margin-left:4px;">
                                                    <a href="javascript:SetDescendingData('creator',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnOwnerNamedown',4)" onmouseover="changeImage(sortImg,'imgcolumnOwnerNamedown',5)" class="commandLabel">
                                                        <img src="/images/btnSortDown.gif" alt="내림차순" title="내림차순" name="imgcolumnOwnerNamedown" align="absmiddle">
                                                    </a>
                                                </span>
                                            </td>
                                            <td class='commandLabel' style='padding-left:16px;' nowrap>
                                                <span style="margin-right:4px;">생성 날짜/시간</span>
                                                <span>
                                                    <a href="javascript:SetAscendingData('creationDate',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnMakingDayup',1)" onmouseover="changeImage(sortImg,'imgcolumnMakingDayup',2)" class="commandLabel">
                                                        <img src="/images/btnSortUP.gif" alt="오름차순" title="오름차순" name="imgcolumnMakingDayup" align="absmiddle">
                                                    </a>
                                                </span>
                                                <span style="margin-left:4px;">
                                                    <img src="/images/btnSortDown-s.gif" alt="내림차순을 선택했습니다." title="내림차순을 선택했습니다." name="imgcolumnMakingDaydown" align="absmiddle">
                                                    <input type="hidden" name="orderBy_property" value="creationDate">
                                                    <input type="hidden" name="orderBy_descendingRequested" value="true">
                                                </span>
                                            </td>
                                            <td class='commandLabel' style='padding-left:16px;' nowrap>
                                                <span style="margin-right:4px;">유효 기간</span>
                                                <span>
                                                    <a href="javascript:SetAscendingData('expirationDate',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnDeadlineup',1)" onmouseover="changeImage(sortImg,'imgcolumnDeadlineup',2)" class="commandLabel">
                                                        <img src="/images/btnSortUP.gif" alt="오름차순" title="오름차순" name="imgcolumnDeadlineup" align="absmiddle">
                                                    </a>
                                                </span>
                                                <span style="margin-left:4px;">
                                                    <a href="javascript:SetDescendingData('expirationDate',document.DocboxListForm);" onmouseout="changeImage(sortImg,'imgcolumnDeadlinedown',4)" onmouseover="changeImage(sortImg,'imgcolumnDeadlinedown',5)" class="commandLabel">
                                                        <img src="/images/btnSortDown.gif" alt="내림차순" title="내림차순" name="imgcolumnDeadlinedown" align="absmiddle">
                                                    </a>
                                                </span>
                                            </td>
                                            <td width="6"/>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                    <div class="float-l mgn-T8px"></div>
                    <input type="hidden" name="wimToken" value="462279011">
                    <input type="hidden" name="offset" value="0">
                    <input type="hidden" name="resultRowBlockSize" value="10">
                    <input type="hidden" name="matrixColSpan" value="4">
                    <input type="hidden" name="urlLang" value="ko">
                    <input type="hidden" name="urlProfile" value="guest">
                    <input type="hidden" name="show" value="thumbnail">
                    <input type="hidden" name="dummy" value="">
                    <input type="hidden" name="applicationType" value="all">
                    <input type="hidden" name="filter_propName" value="">
                    <input type="hidden" name="filter_propValue" value="">
                    <input type="hidden" name="filter_available" value="false">
                    <input type="hidden" name="subParam" value="1">
                    <input type="hidden" name="subReturnDsp" value="1">
                    <input type="hidden" name="goHome" value="">
                    <input type="hidden" name="searchOff_status" value="">
                    <input type="hidden" name="targetDocId" value="">
                    <input type="hidden" name="targetDocPassword" value="">
                    <input type="hidden" name="totalCount" value="0">
                    <input type="hidden" name="useSavedParam" value="">
                    <input type="hidden" name="useInputParam" value="">
                    <input type="hidden" name="confirmText" value="폴더 목록으로 돌아가면 파일 선택이 취소됩니다. 폴더 목록으로 돌아가시겠습니까?">
            </form>
        </div>
        <div class="clear"/>
        <div id="frame_btm" style="margin:0px;clear:both;">
            <p class="Bodder clear">
                <img src="/images/settingDivision.gif" alt="" title="">
            </p>
            <div id='button'>
                <ul>
                    <li>
                        <input type="button" value="뒤로" onClick="javaScript:folderListFormSubmit(1,document.DocboxListForm);" class='defaultButton'></input>
                    </li>
                </ul>
            </div>
        </div>
        <iframe name="downloadFrame" id="downloadFrame" style="display:none;"></iframe>
    </body>
</html>
