## 박스 목록 조회
Request URL
http://192.168.11.193/wcd/api/AppReqGetUserBoxList
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.193:80
Referrer Policy
strict-origin-when-cross-origin
content-length
465
content-type
application/json;charset=utf-8
expires
Thu, 01 Jan 1970 00:00:00 GMT
x-frame-options
SAMEORIGIN
accept
application/json, text/javascript, */*; q=0.01
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
content-length
243
content-type
application/x-www-form-urlencoded; charset=UTF-8
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; usr=F_UOU; cou=
host
192.168.11.193
origin
http://192.168.11.193
referer
http://192.168.11.193/wcd/spa_main.html
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
x-requested-with
XMLHttpRequest
페이로드: {"BoxListCondition":{"SearchKey":"None","WellUse":"false","BoxAttribute":{"Category":"Functional","Type":"User","Attribute":"AllAttribute"},"ObtainCondition":{"Type":"SpecifiedNo","SpecifiedNo":"2"}},"Token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp"}

응답:
{
    "MFP": {
        "Token": "7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp",
        "Result": {
            "ResultInfo": "Ack"
        },
        "BoxInfoList": {
            "ArraySize": "1",
            "BoxInfo": {
                "BoxID": "2",
                "BoxAttribute": {
                    "Category": "Functional",
                    "Type": "User",
                    "Attribute": "Public"
                },
                "Name": "test_name",
                "SearchKey": "Abc",
                "CreatorName": "Public",
                "CreateTime": {
                    "Year": "2026",
                    "Month": "4",
                    "Day": "9",
                    "Hour": "15",
                    "Minute": "40",
                    "Second": "22"
                },
                "ValidPassword": "true",
                "GenFormatAutoCnv": "On",
                "SmbEncryption": "Off",
                "DownloadTimePriority": "false"
            }
        }
    }
}

## 박스 내 파일목록 진입 (비밀번호 입력 완료)
Request URL
http://192.168.11.193/wcd/api/AppReqSetCustomMessage/_105_000_ULU000
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.193:80
Referrer Policy
strict-origin-when-cross-origin
content-length
229
content-type
text/plain;charset=utf-8
expires
Thu, 01 Jan 1970 00:00:00 GMT
set-cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; usr=F_UOU; cou=; path=/wcd
x-frame-options
SAMEORIGIN
accept
application/json, text/javascript, */*; q=0.01
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
content-length
162
content-type
application/x-www-form-urlencoded; charset=UTF-8
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; usr=F_UOU; cou=
host
192.168.11.193
origin
http://192.168.11.193
referer
http://192.168.11.193/wcd/spa_main.html
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
x-requested-with
XMLHttpRequest
페이로드: {"func":"PSL_F_UOUUser_BOX","H_BID":"2","T_BID":"2","P_BPA":"1234","h_token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp","H_PID":"-1","H_IPA":"On","H_BAT":"Public","_":""}
응답: {"MFP":{"TemplateName":"waitmove.xsl","Function":"err","LangNo":"Ko","RedirectUrl":"box_detail.xml","Interval":"100","MyTab":"false","NoMessage":"true","Message":null,"CgiAction":"void","CgiIdentity":"void","CancelDisp":"false"}}


## 박스 내 파일목록 (비밀번호 입력 후):
Request URL
http://192.168.11.193/wcd/box_detail.json
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.193:80
Referrer Policy
strict-origin-when-cross-origin
content-length
9782
content-type
text/plain;charset=utf-8
expires
Thu, 01 Jan 1970 00:00:00 GMT
set-cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; usr=F_UOU; cou=
x-frame-options
SAMEORIGIN
accept
application/json, text/javascript, */*; q=0.01
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
content-length
24
content-type
application/x-www-form-urlencoded; charset=UTF-8
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; usr=F_UOU; cou=
host
192.168.11.193
origin
http://192.168.11.193
referer
http://192.168.11.193/wcd/spa_main.html
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
x-requested-with
XMLHttpRequest
페이로드: waitend=true&TaskNo=0&_=
응답: {"MFP":{"DisplayMode":null,"Token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp","Common":{"SelNo":"Ko","LangNo":"Ko","Favorite":"On","AuthUserName":"Public","LoginServerIndex":"0","LoginMode":"PublicUser","LoginName":null,"EnableWebConnection":"true","PcmSsId":"268","Simulator":"false","CertInstalled":"false","GreekEnable":"true","HebrewEnable":"true","EncryptEnable":"true","JpegEnable":"false","KeyVndBoxEnable":"Off","BoxSort":"false","DN30B1":"Off","DN27B8":"Off","DN38B8":"Off","DN74B7":"Off","DN35B3":"Off","DN35B4":"Off","DN41B2":"Off","DN39B1":"Off","DN42B8":"Off","DN61B3":"Off","DN75B3":"Off","DN47B7":"Off","DN49B7":"Off","DN70B6":"Off","DN52B5":"Off","DN64B2":"Off","DN65B7":"Off","DN65B2":"Off","DN69B2":"Off","DN69B6":"Off","DN80B4":"On","DN71B4":["Off","Off"],"DN71B7":"Off","DN92B7":"Off","DN72B3":"Off","DN72B5":"Off","DN73B1":"Off","DN75B5":"Off","DN75B8":"Off","DN77B6":"Off","DN75B1":"Off","DN215B2":"Off","PswcDirectPrint":"On","DN79B6":"Off","DN79B5":"Off","DN130B1":"Off","DN84B8":"Off","DN4B1":"Off","DN89B8":"Off","DN90B1":"Off","DN90B2":"Off","DN90B3":"Off","DN90B4":"Off","DN90B5":"Off","DN91B1":"Off","DN91B2":"On","DN91B3":"Off","DN87B1":"Off","DN89B4":"Off","DN74B8":"Off","DN76B5":"Off","DN82B1":"Off","DN101B5":"On","DN112B6":"Off","DN107B1":"Off","DN119B3":"Off","DN94B2":"Off","DN109":"2","DN108B3":"Off","IsGeneric":"On","DN111B6":"Off","DN121B4":"Off","DN122B8":"Off","DN135B6":"Off","DN54B7":"Off","DN70B4":"Off","DN142B5":"Off","MyAddressEnable":"Off","KeyDeviceList":{"ArraySize":"5","KeyDevice":[{"Type":"KeyCounter","Status":"False","Installed":"False"},{"Type":"ExternalManagementDevice","Status":"False","Installed":"False"},{"Type":"Vendor","Status":"False","Installed":"False"},{"Type":"ExternalManagementDeviceKM","Status":"False","Installed":"False"},{"Type":"VendorKM","Status":"False","Installed":"False"}]},"Info":{"EngType":"OTHER"},"FuncVer":"3","DisplayErrorCode":"On","SendingDomainLimit":"NoLimit","IsAral65":"True","DisplayID":"NONE","AddressLock":"false","ColorFlag7":"True","isColor":"True","isA4Printer":"False","HDDLessFax":"false","SupportFunction":{"AppOption":{"Preview":"On","Stamp":"On","SendOperatorLog":"On","InspectLog":"On","FileTypeNoLicence":"On","FileTypeWithLicence":"On","Fax":"On","BizhubRA":"On","SearchLdap":"On","Bmlinks":"On","UbiquitousStorage":"On","UbiquitousClient":"On","WebBrowser":"On","Iws":"On","UserControlCnt":"On","Overlay":"On","SecurityPrintNoHDD":"Off"},"MfpEquipModel":null,"FwRollbackApplicable":"On","LocalInterfaceKit":"Off","PlainPaperExApplicable":"On","OOXMLFontData":"On"},"SIPRNetPKI":"False","Fiery":"false","EnableServerGroup":"On","EnableSimpleServerGroup":"On","EnableIccServerGroup":"Off","LScaleEnable":"Valid","DataRegistrationExtension":"Off","SecurityLevel":"None","BoxCreatePermission":"On","EnableBrowserInfoUser":"Off","BoxUseSetting":{"PublicBoxPermission":"On","GroupBoxPermission":"On","SecurityDocumentBoxPermission":"On","FilingNumberBoxPermission":"On","RelayBoxPermission":"On","BulletinBoardBoxPermission":"On","PollingSendBoxPermission":"On"},"DistributeSetting":null,"ScanFunctionEnable":"true","MFPSmbFileSharing":"On","MFPSmbServer":"On","MultiLanSupport":"true","VlanUsageSettings":"false","PanelFlag":"false","ReqClientAddr":"192.168.11.184","ReqHostIpAddr":"192.168.11.193","AuthDevice":{"CardActionSetting":null,"CardPinCodeDigit":"4","BioActionSetting":null,"BioPinCodeDigit":"4"},"IdPAuthUserData":"false","System":{"ProductName":"SINDOH D450","ProductID":"1.3.6.1.4.1.18334.1.2.1.2.1.181.3.4","DeviceID":null,"Manufacture":"Generic","Oem":"true","SerialNumber":"800101113840","OtherSerialNumber":{"ScannerId":"AA2JA26AY0268883","AdfId":null,"LctId":null,"PaperEjectionId":null,"DuplexId":null,"VendorId":null,"Fax1Id":null,"Fax2Id":null,"Fax3Id":null,"Fax4Id":null,"RuId":null,"ZuId":null,"PkId":null,"PiId":null},"ExternalIfMode":"None","ControllerInfoList":{"ArraySize":"1","ControllerInfo":{"Type":"Printer","Name":"SINDOH D450","Version":"AA2J0Y0-3000-G00-N1"}},"SupportFunction":{"Copy":"true","Print":"true","Scan":"true","Fax":"true","Fax2":"false","Fax3":"Off","Fax4":"Off","Ifax":"false","IpAddressFax":"Off","JobNumberDisplayFunc":"On","UsbHostBoard":"On","Dsc":"Off","Dsc2":"Off","Bluetooth":"Off","DualScan":"Off","InternalWebServer":"On","CustomDocumentMode":"Off","ExpansionNetworkAdapter":"Off","DsBoard":"On","SlidePanel":"Off","AllowIPFilterSetting":"On","BillingCounter":"Off","PowerSaveTimeUpperLimitChange":"Off","TxReportImageAttach":"Off","NetworkIf":"SingleWired","TouchPanel":"Electrostatic","ManualStapleFunction":"Off","IPAddressFax":"false","FaxBoard":"true","FaxBoard2":"false","FaxBoard3":"false","FaxBoard4":"false"},"GeneralContact":{"SiteName":null,"Info":null,"ProductHelpUrl":null,"CorpUrl":null,"SupplyInfo":null,"PhoneNumber":null,"EmailAddress":null,"UtilityLink":null,"OnlineHelpUrl":null,"DriverUrl":null},"UserContact":{"Contact":null,"Name":null,"Location":"800101113840","InternalNumber":null},"Time":{"Year":"2026","Month":"4","Day":"13","Hour":"13","Minute":"33","Second":"45","TimeZone2":{"GmtDirection":"East","Hour":"9","Minute":"0"},"TimeZone":"East_9_00"},"VoiceGuide":{"Enable":"Off"},"ISOCertification":"false","ExternalIfMode2":"None","FunctionStatus":{"FunctionCode":"0000000000000000000000000000000000000000000000000000010011010000","WaitReboot":"0000000000000000000000000000000000000000000000000000000000000000","FunctionCodeReverse":"0000101100100000000000000000000000000000000000000000000000000000"},"Oem2":"true"},"DeviceInfo":{"Option":{"Hdd":{"Installed":"true","Capacity":"241903"},"Ssd":{"Installed":"exist"},"Memory":{"Installed":"true","Capacity":"8192"},"Duplex":{"Installed":"true","Type":"Cycle"},"Adf":{"Installed":"true","Type":"Duplex"},"CardAuthenticationDevice":"Off","BiometricAuthenticationDevice":"Off","LoadableDevice":null,"EnableCardType":null,"WirelessAdapterType":"Notattached"}},"DeviceStatus":{"ScanStatus":"210036","PrintStatus":"110036","Processing":"0","JamCode":":","NetworkErrorStatus":"48","KmSaasgw":"2","HddMirroringErrorStatus":"48","DisplayJamCode":"Off"},"Service":{"Info":{"MarketArea":"Europe"},"Setting":{"AuthSetting":{"SynchronizedTrack":null,"AuthMode":{"ListOn":"false","PublicUser":"true","BoxAdmin":"false","AuthorityPermission":"Off","SendAddressLimit":"Off","AuthType":"None","MiddleServerUse":"Off","DefaultAuthType":null},"TrackMode":{"TrackType":"None"},"CommonMode":{"NoAuthPrintOn":"false"},"UserAndTrack":{"ColorManage":"Color"},"AuthDataSearch":{"Enable":"Off"}},"SystemConnection":{"AdminSend":"Off","PrefixSuffix":"Off","MobilePrint":"On","ConnectApplication":{"MyPanelConnect":"On","MySpoolConnect":"On"},"ChangeUserDataPermission":"Off","MobileConnection":{"QrCodeDisplay":"Off","NfcSetting":"Off","BluetoothLeSetting":"Off","QrCodeSetting":{"WirelessConnection":{"Enable":"Off","ConnectionType":"MfpWirelessLanSetting","IndividualSetting":{"Ssid":null,"AuthEncryptAlgorithm":"None","WepKey":{"InputMethod":"Ascii64bit","Key":null},"PassPhraseString":"Ascii","PassPhrase":null}}},"AarStartingApp":"PageScopeMobile"},"WebAPIPortNo":"60000"},"General":{"KeyMode":"None","IntegrationManagementAuth":"Off","Panel":{"Language":"Ko"},"Security":{"PasswordAgreement":"false","SecurityLevel":"None","SecurePrintLimited":"Off","CopyGuardEnable":"Off","PasswordCopyEnable":"Off","BoxUseSetting":{"PublicBoxPermission":"On","GroupBoxPermission":"On","SecurityDocumentBoxPermission":"On","FilingNumberBoxPermission":"On","RelayBoxPermission":"On","BulletinBoardBoxPermission":"On","PollingSendBoxPermission":"On"}},"ForcedPrintInPrinting_Jimon":"Off","ForcedPrintInPrinting_CopyGuard":"Off","ForcedPrintInPrinting_PasswordCopy":"Off","ForcedPrintInPrinting_Watermark":"Off"}}}},"DeviceInfo":{"ExtensionFunction":{"MyPanel":"Off","MyAddress":"Off","CompactXps":"On","SearchablePdfDictionary":"On","FontPdf_A":"On","VoiceData":"Off","IPFaxT38Library":"Off"}},"DocumentNumberTotal":"0","Job":{"BoxInfoList":{"BoxInfo":{"BoxID":"2","BoxAttribute":{"Category":"Functional","Type":"User","Attribute":"Public"},"Name":"test_name","SearchKey":"Abc","Confidential":"false","CreatorName":"Public","CreateTime":{"Year":"2026","Month":"4","Day":"9","Hour":"15","Minute":"40","Second":"22"},"NumberOfFile":"1","LifeTime":"0","LifeTimeMinute":"0","ValidPassword":"true","GenFormatAutoCnv":"On","SmbEncryption":"Off","DownloadTimePriority":"false"},"BoxJobInfoList":{"BoxJobInfo":{"BoxJobID":"1","JobName":"S800101113826041008490","JobTime":{"CreateTime":{"Year":"2026","Month":"04","Day":"10","Hour":"08","Minute":"49"}},"KindOfJob":{"JobType":"Send"},"FileType":["CompactPdf","CompactPdf"],"FileTypeBoxSend":"CompactPdf","CPdfConversion":"On","JobAllowedOperationInfoList":{"ArraySize":"6","AllowedFunction":["Print","SendEmail","SendSMB","SendFTP","SendFax","Get"]},"RestrictChangeOutputColor":"Off","RestrictChangeOutputSize":"Off","CopyProtectDetect":"0","RxMode":"Non","ColorType":"FullColor","DocumentNumber":"1","EncipherPdfSetting":{"EncipherLevel":"None"},"Resolution":{"X":"300","Y":"300"},"PageShoot":"Off"}}},"BoxCreatePermission":"On"},"AllDocumentList":"@1@","PrintDocumentList":"@1@","SnedDocumentList":"@1@","DownloadDocumentList":"@1@","FaxSendDocumentList":"@1@","FullColorDocumentList":"@1@","Service":{"Setting":{"Security":{"DeleteBoxDocument":{"Enable":"On","LifeTime":"Day1"}},"WindowsNetwork":{"ServerSetting":{"ServerCommon":{"SmbServer":"On"},"FileSharingSetting":{"SmbFileSharing":"On"}}},"BoxConfiguration":{"GenFormatAutoCnvBoxConfiguration":{"NumberOfBox":"8","MaxNumberOfBox":"300"}},"Fax":{"ReceiveDataProtectSetting":{"DataProtectType":"NotProtect","DeletePasswordExist":"false"}}}},"BoxExport":"false","OpeSelect":"AllDocument","DocSelect":null,"CfdSelect":null,"DispType":"Thumbnail","CurrentPage":"-1","DocStructure":{"Page":"1"}}}
* 예시에서 파일명은 'S800101113826041008490'

## 다운로드 요청
Request URL
http://192.168.11.193/wcd/api/AppReqSetCustomMessage/_105_000_ULU004
Request Method
POST
Status Code
200 OK
Remote Address
192.168.11.193:80
Referrer Policy
strict-origin-when-cross-origin
content-length
309
content-type
text/plain;charset=utf-8
expires
Thu, 01 Jan 1970 00:00:00 GMT
set-cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; cou=; usr=F_UOU_FileDownload; path=/wcd
x-frame-options
SAMEORIGIN
accept
application/json, text/javascript, */*; q=0.01
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
content-length
394
content-type
application/x-www-form-urlencoded; charset=UTF-8
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; notChange=; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; cou=; usr=F_UOU_FileDownload
host
192.168.11.193
origin
http://192.168.11.193
referer
http://192.168.11.193/wcd/spa_main.html
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
x-requested-with
XMLHttpRequest
페이로드: {"H_TAB":"","func":"PSL_F_UOU_DWN","h_token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp","H_BOX":"2","H_BPA":"","H_BTY":"User","H_PID":"-1","H_DTY":"FileDownload","H_XTP":"Thumbnail","H_FMT":"CompactPdf","H_JLS":"@1@","H_JNL":"\tS800101113826041008490\t","H_JOR":"@1@","H_DCN":"1","C_GFA":"On","F_UOU_S_FOR":"CompactPdf","S_OUT":"Off","S_LRP":"Off","S_PDA":"Off","F_UOU_R_PAG":"MultiPage","R_SPG":"Off"}
응답: {"MFP":{"TemplateName":"wait.xsl","Function":"err","LangNo":"Ko","RedirectUrl":"progress","Interval":"2500","MyTab":"false","NoMessage":"false","Message":{"Item":{"@Code":"DeviceExportExec"}},"CgiAction":"user.cgi","CgiIdentity":"PSL_F_UOU_DCA","Token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp","CancelDisp":"true"}}

## 파일 다운로드 (로딩 끝난 후)
Request URL
http://192.168.11.193/wcd/doc/S800101113826041008490.pdf?func=PSL_F_UOU_DLD&h_token=7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp&cginame1=doc%2FS800101113826041008490.pdf&cginame2=doc%2FS800101113826041008490.pdf&H_BAK=0&H_TAB=&H_DLV=
Request Method
GET
Status Code
200 OK
Remote Address
192.168.11.193:80
Referrer Policy
strict-origin-when-cross-origin
content-disposition
attachment;filename*=UTF-8''S800101113826041008490.pdf
content-type
application/octet-stream
expires
Thu, 01 Jan 1970 00:00:00 GMT
transfer-encoding
chunked
accept
text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
accept-encoding
gzip, deflate
accept-language
ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7
connection
keep-alive
cookie
bv=Chrome/146.0.0.0; pf=PC; uatype=NN; lang=Ko; favmode=false; vm=Html; key=; selno=Ko; InitialTransitionScreen=; loginUserName=; hostChange=; sourcePage=1; abbrRedojobingStatus=allow; loginState=true; box_dsp=Setting; webUI=new; menuType=Public; logoutIF=user.cgi; ID=5bPAPAyueUF7nyhgKCESmywQaoWtGa7v; abbrCheckCookieFlg=true; cou=; usr=F_UOU_FileDownload
host
192.168.11.193
referer
http://192.168.11.193/wcd/spa_contents_frame.tmpl.html
upgrade-insecure-requests
1
user-agent
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36
페이로드: func=PSL_F_UOU_DLD&h_token=7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp&cginame1=doc%2FS800101113826041008490.pdf&cginame2=doc%2FS800101113826041008490.pdf&H_BAK=0&H_TAB=&H_DLV=


## 실제 박스 내 파일목록 조회 과정 기록
- 최초 box_detail.json 응답:
{"MFP":{"TemplateName":"wait.xsl","Function":"err","LangNo":"Ko","RedirectUrl":"waitmsg","Interval":"100","MyTab":"false","NoMessage":"false","Message":{"Item":{"@Code":"DeviceExportExec"}},"CgiAction":"waitmsg","CgiIdentity":"void","CancelDisp":"false","ParamName":"TaskNo","ParamValue":"0"}}

- waitmsg
Request URL
http://192.168.11.193/wcd/waitmsg
Request Method
POST
Status Code
200 OK
페이로드: TaskNo=0&_=
응답: {"MFP":{"TemplateName":"wait.xsl","Function":"err","LangNo":"Ko","RedirectUrl":"waitmsg","Interval":"2000","MyTab":"false","NoMessage":"false","Message":{"Item":{"@Code":"DeviceExportExec"}},"CgiAction":"waitmsg","CgiIdentity":"void","CancelDisp":"false","ParamName":"TaskNo","ParamValue":"0"}}

-waitmsg
Request URL
http://192.168.11.193/wcd/waitmsg
Request Method
POST
Status Code
200 OK
페이로드: TaskNo=0&_=
응답: {"MFP":{"RedirectUrl":"box_detail.json","waitend":"true","TaskNo":"0"}}

- 두번째 box_detail.json 응답:
{"MFP":{"DisplayMode":null,"Token":"7nnXpiSxdsEdy8yYBlJNBl7NmaqRgBPp","Common":{"SelNo":"Ko","LangNo":"Ko","Favorite":"On","AuthUserName":"Public","LoginServerIndex":"0","LoginMode":"PublicUser","LoginName":null,"EnableWebConnection":"true","PcmSsId":"470","Simulator":"false","CertInstalled":"false","GreekEnable":"true","HebrewEnable":"true","EncryptEnable":"true","JpegEnable":"false","KeyVndBoxEnable":"Off","BoxSort":"false","DN30B1":"Off","DN27B8":"Off","DN38B8":"Off","DN74B7":"Off","DN35B3":"Off","DN35B4":"Off","DN41B2":"Off","DN39B1":"Off","DN42B8":"Off","DN61B3":"Off","DN75B3":"Off","DN47B7":"Off","DN49B7":"Off","DN70B6":"Off","DN52B5":"Off","DN64B2":"Off","DN65B7":"Off","DN65B2":"Off","DN69B2":"Off","DN69B6":"Off","DN80B4":"On","DN71B4":["Off","Off"],"DN71B7":"Off","DN92B7":"Off","DN72B3":"Off","DN72B5":"Off","DN73B1":"Off","DN75B5":"Off","DN75B8":"Off","DN77B6":"Off","DN75B1":"Off","DN215B2":"Off","PswcDirectPrint":"On","DN79B6":"Off","DN79B5":"Off","DN130B1":"Off","DN84B8":"Off","DN4B1":"Off","DN89B8":"Off","DN90B1":"Off","DN90B2":"Off","DN90B3":"Off","DN90B4":"Off","DN90B5":"Off","DN91B1":"Off","DN91B2":"On","DN91B3":"Off","DN87B1":"Off","DN89B4":"Off","DN74B8":"Off","DN76B5":"Off","DN82B1":"Off","DN101B5":"On","DN112B6":"Off","DN107B1":"Off","DN119B3":"Off","DN94B2":"Off","DN109":"2","DN108B3":"Off","IsGeneric":"On","DN111B6":"Off","DN121B4":"Off","DN122B8":"Off","DN135B6":"Off","DN54B7":"Off","DN70B4":"Off","DN142B5":"Off","MyAddressEnable":"Off","KeyDeviceList":{"ArraySize":"5","KeyDevice":[{"Type":"KeyCounter","Status":"False","Installed":"False"},{"Type":"ExternalManagementDevice","Status":"False","Installed":"False"},{"Type":"Vendor","Status":"False","Installed":"False"},{"Type":"ExternalManagementDeviceKM","Status":"False","Installed":"False"},{"Type":"VendorKM","Status":"False","Installed":"False"}]},"Info":{"EngType":"OTHER"},"FuncVer":"3","DisplayErrorCode":"On","SendingDomainLimit":"NoLimit","IsAral65":"True","DisplayID":"NONE","AddressLock":"false","ColorFlag7":"True","isColor":"True","isA4Printer":"False","HDDLessFax":"false","SupportFunction":{"AppOption":{"Preview":"On","Stamp":"On","SendOperatorLog":"On","InspectLog":"On","FileTypeNoLicence":"On","FileTypeWithLicence":"On","Fax":"On","BizhubRA":"On","SearchLdap":"On","Bmlinks":"On","UbiquitousStorage":"On","UbiquitousClient":"On","WebBrowser":"On","Iws":"On","UserControlCnt":"On","Overlay":"On","SecurityPrintNoHDD":"Off"},"MfpEquipModel":null,"FwRollbackApplicable":"On","LocalInterfaceKit":"Off","PlainPaperExApplicable":"On","OOXMLFontData":"On"},"SIPRNetPKI":"False","Fiery":"false","EnableServerGroup":"On","EnableSimpleServerGroup":"On","EnableIccServerGroup":"Off","LScaleEnable":"Valid","DataRegistrationExtension":"Off","SecurityLevel":"None","BoxCreatePermission":"On","EnableBrowserInfoUser":"Off","BoxUseSetting":{"PublicBoxPermission":"On","GroupBoxPermission":"On","SecurityDocumentBoxPermission":"On","FilingNumberBoxPermission":"On","RelayBoxPermission":"On","BulletinBoardBoxPermission":"On","PollingSendBoxPermission":"On"},"DistributeSetting":null,"ScanFunctionEnable":"true","MFPSmbFileSharing":"On","MFPSmbServer":"On","MultiLanSupport":"true","VlanUsageSettings":"false","PanelFlag":"false","ReqClientAddr":"192.168.11.184","ReqHostIpAddr":"192.168.11.193","AuthDevice":{"CardActionSetting":null,"CardPinCodeDigit":"4","BioActionSetting":null,"BioPinCodeDigit":"4"},"IdPAuthUserData":"false","System":{"ProductName":"SINDOH D450","ProductID":"1.3.6.1.4.1.18334.1.2.1.2.1.181.3.4","DeviceID":null,"Manufacture":"Generic","Oem":"true","SerialNumber":"800101113840","OtherSerialNumber":{"ScannerId":"AA2JA26AY0268883","AdfId":null,"LctId":null,"PaperEjectionId":null,"DuplexId":null,"VendorId":null,"Fax1Id":null,"Fax2Id":null,"Fax3Id":null,"Fax4Id":null,"RuId":null,"ZuId":null,"PkId":null,"PiId":null},"ExternalIfMode":"None","ControllerInfoList":{"ArraySize":"1","ControllerInfo":{"Type":"Printer","Name":"SINDOH D450","Version":"AA2J0Y0-3000-G00-N1"}},"SupportFunction":{"Copy":"true","Print":"true","Scan":"true","Fax":"true","Fax2":"false","Fax3":"Off","Fax4":"Off","Ifax":"false","IpAddressFax":"Off","JobNumberDisplayFunc":"On","UsbHostBoard":"On","Dsc":"Off","Dsc2":"Off","Bluetooth":"Off","DualScan":"Off","InternalWebServer":"On","CustomDocumentMode":"Off","ExpansionNetworkAdapter":"Off","DsBoard":"On","SlidePanel":"Off","AllowIPFilterSetting":"On","BillingCounter":"Off","PowerSaveTimeUpperLimitChange":"Off","TxReportImageAttach":"Off","NetworkIf":"SingleWired","TouchPanel":"Electrostatic","ManualStapleFunction":"Off","IPAddressFax":"false","FaxBoard":"true","FaxBoard2":"false","FaxBoard3":"false","FaxBoard4":"false"},"GeneralContact":{"SiteName":null,"Info":null,"ProductHelpUrl":null,"CorpUrl":null,"SupplyInfo":null,"PhoneNumber":null,"EmailAddress":null,"UtilityLink":null,"OnlineHelpUrl":null,"DriverUrl":null},"UserContact":{"Contact":null,"Name":null,"Location":"800101113840","InternalNumber":null},"Time":{"Year":"2026","Month":"4","Day":"13","Hour":"14","Minute":"18","Second":"27","TimeZone2":{"GmtDirection":"East","Hour":"9","Minute":"0"},"TimeZone":"East_9_00"},"VoiceGuide":{"Enable":"Off"},"ISOCertification":"false","ExternalIfMode2":"None","FunctionStatus":{"FunctionCode":"0000000000000000000000000000000000000000000000000000010011010000","WaitReboot":"0000000000000000000000000000000000000000000000000000000000000000","FunctionCodeReverse":"0000101100100000000000000000000000000000000000000000000000000000"},"Oem2":"true"},"DeviceInfo":{"Option":{"Hdd":{"Installed":"true","Capacity":"241903"},"Ssd":{"Installed":"exist"},"Memory":{"Installed":"true","Capacity":"8192"},"Duplex":{"Installed":"true","Type":"Cycle"},"Adf":{"Installed":"true","Type":"Duplex"},"CardAuthenticationDevice":"Off","BiometricAuthenticationDevice":"Off","LoadableDevice":null,"EnableCardType":null,"WirelessAdapterType":"Notattached"}},"DeviceStatus":{"ScanStatus":"210036","PrintStatus":"110036","Processing":"0","JamCode":":","NetworkErrorStatus":"48","KmSaasgw":"2","HddMirroringErrorStatus":"48","DisplayJamCode":"Off"},"Service":{"Info":{"MarketArea":"Europe"},"Setting":{"AuthSetting":{"SynchronizedTrack":null,"AuthMode":{"ListOn":"false","PublicUser":"true","BoxAdmin":"false","AuthorityPermission":"Off","SendAddressLimit":"Off","AuthType":"None","MiddleServerUse":"Off","DefaultAuthType":null},"TrackMode":{"TrackType":"None"},"CommonMode":{"NoAuthPrintOn":"false"},"UserAndTrack":{"ColorManage":"Color"},"AuthDataSearch":{"Enable":"Off"}},"SystemConnection":{"AdminSend":"Off","PrefixSuffix":"Off","MobilePrint":"On","ConnectApplication":{"MyPanelConnect":"On","MySpoolConnect":"On"},"ChangeUserDataPermission":"Off","MobileConnection":{"QrCodeDisplay":"Off","NfcSetting":"Off","BluetoothLeSetting":"Off","QrCodeSetting":{"WirelessConnection":{"Enable":"Off","ConnectionType":"MfpWirelessLanSetting","IndividualSetting":{"Ssid":null,"AuthEncryptAlgorithm":"None","WepKey":{"InputMethod":"Ascii64bit","Key":null},"PassPhraseString":"Ascii","PassPhrase":null}}},"AarStartingApp":"PageScopeMobile"},"WebAPIPortNo":"60000"},"General":{"KeyMode":"None","IntegrationManagementAuth":"Off","Panel":{"Language":"Ko"},"Security":{"PasswordAgreement":"false","SecurityLevel":"None","SecurePrintLimited":"Off","CopyGuardEnable":"Off","PasswordCopyEnable":"Off","BoxUseSetting":{"PublicBoxPermission":"On","GroupBoxPermission":"On","SecurityDocumentBoxPermission":"On","FilingNumberBoxPermission":"On","RelayBoxPermission":"On","BulletinBoardBoxPermission":"On","PollingSendBoxPermission":"On"}},"ForcedPrintInPrinting_Jimon":"Off","ForcedPrintInPrinting_CopyGuard":"Off","ForcedPrintInPrinting_PasswordCopy":"Off","ForcedPrintInPrinting_Watermark":"Off"}}}},"DeviceInfo":{"ExtensionFunction":{"MyPanel":"Off","MyAddress":"Off","CompactXps":"On","SearchablePdfDictionary":"On","FontPdf_A":"On","VoiceData":"Off","IPFaxT38Library":"Off"}},"DocumentNumberTotal":"0","Job":{"BoxInfoList":{"BoxInfo":{"BoxID":"2","BoxAttribute":{"Category":"Functional","Type":"User","Attribute":"Public"},"Name":"test_name","SearchKey":"Abc","Confidential":"false","CreatorName":"Public","CreateTime":{"Year":"2026","Month":"4","Day":"9","Hour":"15","Minute":"40","Second":"22"},"NumberOfFile":"1","LifeTime":"0","LifeTimeMinute":"0","ValidPassword":"true","GenFormatAutoCnv":"On","SmbEncryption":"Off","DownloadTimePriority":"false"},"BoxJobInfoList":{"BoxJobInfo":{"BoxJobID":"1","JobName":"S800101113826041008490","JobTime":{"CreateTime":{"Year":"2026","Month":"04","Day":"10","Hour":"08","Minute":"49"}},"KindOfJob":{"JobType":"Send"},"FileType":["CompactPdf","CompactPdf"],"FileTypeBoxSend":"CompactPdf","CPdfConversion":"On","JobAllowedOperationInfoList":{"ArraySize":"6","AllowedFunction":["Print","SendEmail","SendSMB","SendFTP","SendFax","Get"]},"RestrictChangeOutputColor":"Off","RestrictChangeOutputSize":"Off","CopyProtectDetect":"0","RxMode":"Non","ColorType":"FullColor","DocumentNumber":"1","EncipherPdfSetting":{"EncipherLevel":"None"},"Resolution":{"X":"300","Y":"300"},"PageShoot":"Off"}}},"BoxCreatePermission":"On"},"AllDocumentList":"@1@","PrintDocumentList":"@1@","SnedDocumentList":"@1@","DownloadDocumentList":"@1@","FaxSendDocumentList":"@1@","FullColorDocumentList":"@1@","Service":{"Setting":{"Security":{"DeleteBoxDocument":{"Enable":"On","LifeTime":"Day1"}},"WindowsNetwork":{"ServerSetting":{"ServerCommon":{"SmbServer":"On"},"FileSharingSetting":{"SmbFileSharing":"On"}}},"BoxConfiguration":{"GenFormatAutoCnvBoxConfiguration":{"NumberOfBox":"8","MaxNumberOfBox":"300"}},"Fax":{"ReceiveDataProtectSetting":{"DataProtectType":"NotProtect","DeletePasswordExist":"false"}}}},"BoxExport":"false","OpeSelect":"AllDocument","DocSelect":null,"CfdSelect":null,"DispType":"Thumbnail","CurrentPage":"-1","DocStructure":{"Page":"1"}}}