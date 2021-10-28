using System;

namespace vcdb.MySql.SchemaBuilding.Models
{

    public class MySqlUserDetails
    {
        public string Host { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public MySqlBoolEnum Select_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Insert_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Delete_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Update_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Create_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Drop_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Reload_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Shutdown_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Process_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum File_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Grant_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum References_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Index_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Alter_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Show_db_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Super_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Create_tmp_table_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Lock_tables_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Execute_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Repl_slave_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Repl_client_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Create_view_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Show_view_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Create_routine_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Alter_routine_priv { get; set; } = MySqlBoolEnum.N;
        public MySqlBoolEnum Create_user_priv { get; set; } = MySqlBoolEnum.N;

        public string Ssl_Type { get; set; }

        public MySqlSslTypeEnum SslType
        {
            get
            {
                return string.IsNullOrEmpty(Ssl_Type)
                    ? MySqlSslTypeEnum.None
                    : Enum.Parse<MySqlSslTypeEnum>(Ssl_Type, true);
            }
            set
            {
                if (value == MySqlSslTypeEnum.None)
                {
                    Ssl_Type = "";
                }
                else
                {
                    Ssl_Type = value.ToString().ToUpper();
                }
            }
        }

        public byte[] Ssl_Cipher { get; set; }
        public byte[] x509_issuer { get; set; }
        public byte[] x509_subject { get; set; }

        public int max_questions { get; set; }
        public int max_updates { get; set; }
        public int max_connections { get; set; }
        public int max_user_connections { get; set; }

        public string plugin { get; set; }

        public string authentication_string { get; set; }

        public MySqlBoolEnum password_expired { get; set; }

        public DateTime password_last_changed { get; set; }

        public string password_lifetime { get; set; }

        public MySqlBoolEnum account_locked { get; set; }
    }
}
