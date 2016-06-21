-----------------------------------------------------------------
--GET DETAIL OF ALL DATABASE IN SERVER (DATA SOURCE)
-----------------------------------------------------------------
SELECT *
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_CATALOG = 'QLSV5T'

-----------------------------------------------------------------
--GET DETAIL OF ALL TABLES IN CURRENT DATABASE
-----------------------------------------------------------------
SELECT tbl.TABLE_NAME, col.COLUMN_NAME, col.DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS col
JOIN (
		SELECT tc.TABLE_NAME 
		FROM INFORMATION_SCHEMA.TABLES tc
		WHERE 
			tc.TABLE_NAME != 'sysdiagrams'
		) tbl ON col.TABLE_NAME = tbl.TABLE_NAME

-----------------------------------------------------------------
--GET PRIMARY KEY OF TABLE NAME ---TABLE_NAME--- AND CURRENT DATABASE
-----------------------------------------------------------------
SELECT 
    cu.CONSTRAINT_NAME, 
    cu.COLUMN_NAME 
FROM 
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu 
	JOIN (
			SELECT tc.*
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
            WHERE 
                tc.TABLE_NAME = '---TABLE_NAME---' 
                AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
	     ) tc ON tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
