SELECT DISTINCT dbo.SLG_BASIC_OVERTIME.SEQUENCE_NUMBER AS SLG#, guru.CustomersVisibleInGuru.Name, dbo.SLG_BASIC_OVERTIME.CONTACT_TITLE,
					dbo.SLG_BASIC_OVERTIME.LOG_STATUS_C AS Status, dbo.SLG_BASIC_OVERTIME.LAST_UPDATE_INSTANT, dbo.SLG_BASIC_OVERTIME.CREATION_INSTANT, 
					dbo.SLG_BASIC_OVERTIME.PRIORITY_C, dbo.SLG_EXTENDED_PROPE.DATE_OF_CONTACT

FROM guru.CustomersVisibleInGuru
		INNER JOIN dbo.SLG_BASIC_OVERTIME on guru.CustomersVisibleInGuru.CUSTOMER_NUMBER = dbo.SLG_BASIC_OVERTIME.CUSTOMER_NUMBER
		INNER JOIN dbo.TLG_EMP_CURRENT on dbo.SLG_BASIC_OVERTIME.BATON_HOLDER = dbo.TLG_EMP_CURRENT.EMPLOYEE_ID
		INNER JOIN dbo.SLG_EXTENDED_PROPE on dbo.SLG_BASIC_OVERTIME.CUSTOMER_NUMBER = dbo.SLG_EXTENDED_PROPE.CUSTOMER_NUMBER

WHERE (guru.CustomersVisibleInGuru.Name = 'Bronson Healthcare Group' AND dbo.SLG_BASIC_OVERTIME.CONTACT_TYPE = '95'  AND dbo.SLG_BASIC_OVERTIME.BATON_HOLDER = '18199' AND dbo.SLG_BASIC_OVERTIME.SEQUENCE_NUMBER = '2278051')--dbo.SLG_BASIC_OVERTIME.LOG_STATUS_C != '300')