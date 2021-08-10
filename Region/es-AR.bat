reg query "HKCU\Control Panel\International"

REG ADD "HKCU\Control Panel\International" /t REG_SZ /v Locale /d 00002C0A /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v LocaleName /d es-AR /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v s1159 /d "a. m." /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v s2359 /d "p. m." /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sCountry /d Argentina /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sCurrency /d $ /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sDate /d / /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sDecimal /d , /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sGrouping /d 3;0 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sLanguage /d ESS /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sList /d ; /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sLongDate /d "dddd, d 'de' MMMM 'de' yyyy" /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sMonDecimalSep /d , /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sMonGrouping /d 3;0 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sMonThousandSep /d . /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sNativeDigits /d 0123456789 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sNegativeSign /d - /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sPositiveSign /d "" /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sShortDate /d d/M/yyyy /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sThousand /d . /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sTime /d : /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sTimeFormat /d HH:mm:ss /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sShortTime /d HH:mm /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v sYearMonth /d "MMMM 'de' yyyy" /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iCalendarType /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iCountry /d 54 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iCurrDigits /d 2 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iCurrency /d 2 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iDate /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iDigits /d 2 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v NumShape /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iFirstDayOfWeek /d 6 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iFirstWeekOfYear /d 0 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iLZero /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iMeasure /d 0 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iNegCurr /d 9 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iNegNumber /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iPaperSize /d 9 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iTime /d 1 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iTimePrefix /d 0 /f
REG ADD "HKCU\Control Panel\International" /t REG_SZ /v iTLZero /d 1 /f

CALL RUNDLL32.EXE USER32.DLL,UpdatePerUserSystemParameters ,1 ,True

reg query "HKCU\Control Panel\International"