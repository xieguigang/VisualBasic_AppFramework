﻿Imports Microsoft.VisualBasic.Data.csv

Module MySQLImportsTest
    Sub Main()
        Dim result = MySQL.ImportsMySQLDump("C:\Users\xieguigang\Documents\dumps\Dump20171029\kb_go_term_synonym.sql").ToArray


        Pause()
    End Sub
End Module
