SELECT DISTINCT CustomerEpicInfo.CUSTOMER_NAME AS Customer, 
				SLGInfo.SEQUENCE_NUMBER AS SLG, 
				SLGInfo.CONTACT_TITLE AS Title,
				SLGInfo.LOG_STATUS_C AS Status, 
				SLGInfo.LAST_UPDATE_INSTANT AS LastUpdate,
				SLGInfo.PRIORITY_C AS PriorityCode, 
				CustomerEpicInfo.EmpID AS PerfID,
				SUBSTRING(CustomerEpicInfo.EmpName, CHARINDEX(',', CustomerEpicInfo.EmpName) + 1, 20) + SUBSTRING(' ' + CustomerEpicInfo.EmpName, 1, CHARINDEX(',', CustomerEpicInfo.EmpName)) as PerfTSName

FROM dbo.V_SLG_Epic_Contacts as CustomerEpicInfo
		INNER JOIN dbo.SLG_BASIC_OVERTIME as SLGInfo on CustomerEpicInfo.CUSTOMER_NUMBER = SLGInfo.CUSTOMER_NUMBER

WHERE (SLGInfo.BATON_HOLDER = @tlgID AND SLGInfo.CONTACT_TYPE = '50' AND SLGInfo.LOG_STATUS_C != '300' AND CustomerEpicInfo.RoleTitle = 'PERFORMANCE' AND CustomerEpicInfo.RoleModTitle = 'Primary')