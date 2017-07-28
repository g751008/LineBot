using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.Configuration;

namespace WebApplication1.Controllers
{

    #region "資料庫物件AjaxDB"
    /// <summary>
    /// 資料庫物件
    /// </summary>
    /// <remarks></remarks>
    internal class AjaxLib
    {

        #region "Fileds(變數)"
        private SqlCommand _cmd = new SqlCommand();
        private SqlConnection _cn = new SqlConnection();
        private SqlDataAdapter _da;
        private SqlParameterCollection _spc;
        //預設為預存程序名稱
        #endregion
        private CommandType _ct = CommandType.StoredProcedure;

        #region "屬性"

        /// <summary>
        /// SqlConnection
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SqlConnection Conn
        {
            get { return this._cn; }
        }

        /// <summary>
        /// SqlCommand
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SqlCommand Cmd
        {
            get { return this._cmd; }
        }

        /// <summary>
        /// SqlDataAdapter
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SqlDataAdapter DataAdapter
        {
            get { return this._da; }
        }


        //連線字串
        public string ConnectionString
        {
            get { return this._cn.ConnectionString; }
            set { this._cn.ConnectionString = value; }
        }

        //命令字串
        public string CommandText
        {
            get { return this._cmd.CommandText; }
            set { this._cmd.CommandText = value; }
        }

        //CommandType
        //Public Property cmdType() As CommandType
        //    Get
        //        Return Me._ct
        //    End Get
        //    Set(ByVal value As CommandType)
        //        Me._ct = value
        //    End Set
        //End Property

        public CommandType cmdType
        {
            set { this._ct = value; }
        }



        #endregion

        #region "建構子"
        public AjaxLib()
        {
            this._cn.ConnectionString = AjaxUtility.MyConnStr;
            this._cmd.Connection = this._cn;
            //Me._da.SelectCommand.Connection = Me._cn
            this._da = new SqlDataAdapter(this._cmd);
        }

        //Public Sub New(ByVal cmdType As CommandType)
        //    Me._cn.ConnectionString = AjaxUtility.MyConnStr()
        //    Me._cmd.Connection = Me._cn
        //    Me._da = New SqlDataAdapter(Me._cmd)
        //    Me._ct = cmdType
        //End Sub

        #endregion

        #region "方法"

        /// <summary>
        /// GetDataTable
        /// </summary>
        /// <param name="strCommandText">T-SQL語法或預存程序名稱</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable GetDataTable(string strCommandText)
        {
            this.CommandText = strCommandText;
            this._cmd.CommandType = this._ct;
            this._cn.Open();
            DataTable dt = new DataTable();
            dt.Load(this._cmd.ExecuteReader());
            this._cn.Close();
            return dt;
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="strCommandText">T-SQL語法或預存程序名稱</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object ExecuteScalar(string strCommandText)
        {
            this.CommandText = strCommandText;
            this._cmd.CommandType = this._ct;
            this._cn.Open();
            object obj = this._cmd.ExecuteScalar();
            this._cn.Close();
            return obj;
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="strCommandText">T-SQL語法或預存程序名稱</param>
        /// <remarks></remarks>
        public void ExecuteNonQuery(string strCommandText)
        {
            this.CommandText = strCommandText;
            this._cmd.CommandType = this._ct;
            this._cn.Open();
            using (this._cn)
            {
                this._cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// ExecuteReader
        /// </summary>
        /// <param name="strCommandText">T-SQL語法或預存程序名稱</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public SqlDataReader ExecuteReader(string strCommandText)
        {
            this.CommandText = strCommandText;
            this._cmd.CommandType = this._ct;
            this._cn.Open();
            SqlDataReader sdr = this._cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return sdr;
        }

        /// <summary>
        /// GetDataSet
        /// </summary>
        /// <param name="strCommandText">T-SQL語法或預存程序名稱</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataSet GetDataSet(string strCommandText)
        {
            this._da.SelectCommand.CommandText = strCommandText;
            this._da.SelectCommand.CommandType = this._ct;
            this._da.SelectCommand.CommandTimeout = 90000;
            DataSet ds = new DataSet();
            this._da.Fill(ds);
            return ds;
        }

        #endregion

    }
    #endregion


    #region "通用物件AjaxUtility"
    /// <summary>
    /// 通用物件
    /// </summary>
    /// <remarks></remarks>
    public class AjaxUtility
    {
        /// <summary>
        /// 資料庫連接字串
        /// </summary>
        /// <value></value>
        /// <returns>回傳資料庫連線字串</returns>
        /// <remarks></remarks>
        public static string MyConnStr
        {
            get { return AjaxSecurity.Decrypt(ConfigurationManager.ConnectionStrings["MISMonitorConnectionString"].ToString()); }
        }
    }

    #endregion


    #region "加解密物件AjaxSecurity"
    /// <summary>
    /// 加解密物件
    /// </summary>
    /// <remarks></remarks>
    public class AjaxSecurity
    {
        //宣告資料加密標準 (DES) 演算法的秘密金鑰。
        static string _mstrKey = "AJAXSOFT";
        //宣告對稱演算法的初始化向量 (IV)。
        static string _mstrIV = "AJAXASIC";
        /// <summary>
        /// 一般字串加密(DES加密)
        /// </summary>
        /// <param name="strValue">字串String</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Encrypt(string strValue)
        {

            //將加密字串、Key與IV轉為所指定的由 base 64 數字所組成之值的 String 表示轉換為相等的 8 位元不帶正負號的整數陣列。
            byte[] buffer = Encoding.Default.GetBytes(HttpUtility.HtmlDecode(strValue));
            byte[] key = Encoding.Default.GetBytes(_mstrKey);
            byte[] iv = Encoding.Default.GetBytes(_mstrIV);

            //宣告ms為記憶體為資料來源的資料流。
            MemoryStream ms = new MemoryStream();

            //定義包裝函式 (Wrapper) 物件，以存取資料加密標準 (DES) 演算法的密碼編譯服務供應者 (CSP) 版本。
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            //使用指定的 Key 和初始化向量 (IV)，建立對稱資料加密標準 (DES) 加密子物件。
            ICryptoTransform transform = des.CreateEncryptor(key, iv);

            //定義連結資料流到密碼編譯轉換的資料流。建構函式==>CryptoStream(要在其上執行密碼編譯轉換的資料流,要在資料流上執行的密碼編譯轉換,密碼編譯資料流的模式)
            CryptoStream encStream = new CryptoStream(ms, transform, CryptoStreamMode.Write);

            //encStream.Write(從 buffer 複製 count 位元組到目前資料流,開始複製位元組到目前資料流的 buffer 的位元組位移,寫入目前資料流的位元組數目)
            encStream.Write(buffer, 0, buffer.Length);

            //以緩衝區的目前狀態更新基礎資料來源或存放庫，並接著清除緩衝區。
            encStream.FlushFinalBlock();

            //將 8 位元不帶正負號的整數陣列的值，轉換為由 base 64 數字所組成的相等 String 表示。
            return HttpUtility.HtmlEncode(Convert.ToBase64String(ms.ToArray()));

        }

        /// <summary>
        /// DES解密(DES加密過的字串)
        /// </summary>
        /// <param name="strValue">經DES加密過的字串</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Decrypt(string strValue)
        {

            //將加密字串、Key與IV轉為所指定的由 base 64 數字所組成之值的 String 表示轉換為相等的 8 位元不帶正負號的整數陣列。
            byte[] buffer = Convert.FromBase64String(HttpUtility.HtmlDecode(strValue));
            byte[] key = Encoding.Default.GetBytes(_mstrKey);
            byte[] iv = Encoding.Default.GetBytes(_mstrIV);

            //宣告ms為記憶體為資料來源的資料流。
            MemoryStream ms = new MemoryStream();

            //定義包裝函式 (Wrapper) 物件，以存取資料加密標準 (DES) 演算法的密碼編譯服務供應者 (CSP) 版本。
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            //使用指定的 Key 和初始化向量 (IV)，建立對稱資料加密標準 (DES) 解密子物件。
            ICryptoTransform transform = des.CreateDecryptor(key, iv);

            //定義連結資料流到密碼編譯轉換的資料流。建構函式==>CryptoStream(要在其上執行密碼編譯轉換的資料流,要在資料流上執行的密碼編譯轉換,密碼編譯資料流的模式)
            CryptoStream encStream = new CryptoStream(ms, transform, CryptoStreamMode.Write);

            //encStream.Write(從 buffer 複製 count 位元組到目前資料流 ,開始複製位元組到目前資料流的 buffer 的位元組位移,寫入目前資料流的位元組數目)
            encStream.Write(buffer, 0, buffer.Length);

            //以緩衝區的目前狀態更新基礎資料來源或存放庫，並接著清除緩衝區。
            encStream.FlushFinalBlock();

            //將 8 位元不帶正負號的整數陣列的值，轉換為由 base 64 數字所組成的相等 String 表示。
            return HttpUtility.HtmlDecode(Encoding.Default.GetString(ms.ToArray()));

        }
    }

    #endregion

}