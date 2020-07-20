using System.Text;
using System;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Core
{
    public enum UOM
    {
        LB = 1,
        EA = 2
    }
    public partial class MeijerMarkdownCode
    {        
        public string Description { get; set; }
        public UOM UnitOM { get; set; }
        public double Price { get; set; } 

        public void ExtractData(string UPC)
        {

            UPC = (UPC.Substring(0, 1).Equals("2")) ? UPC.Substring(0, 6) + "00000" : UPC.Substring(0, UPC.Length - 1);

            string connStr = @"Data Source=retalixhqdev; Initial Catalog=MEIJER_DB; Integrated Security=SSPI;";
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();

            string SQL = @"SELECT distinct 
            SUBSTRING(im.UPC_EAN, 3, 11) as 'UPC', 
            im.UNIT_OF_MEASURE as 'UOM',
            CONVERT(DECIMAL(15, 2), (ip.ip_unit_price / COALESCE(ip.ip_price_multiple, 1))) as 'Price',
            SUBSTRING(REPLACE(im.description, ',', '~'), 1, 50) as 'Description'
            From item_master im(NOLOCK)
            INNER JOIN vendor_item vi(NOLOCK) ON im.item_id = vi.item_id and vi.record_status < 3
            INNER JOIN smrv_item_price_id sip(NOLOCK) ON vi.item_id = sip.item_id and vi.vi_id = sip.vi_id and vi.v_id = sip.v_id and sip.price_level = 0
            and sip.store_id = 1
            and sip.eff_date = convert(varchar, getdate(), 110)
            INNER JOIN item_price ip(NOLOCK) ON sip.item_price_id = ip.item_price_id
            INNER JOIN store_department sd(NOLOCK) ON im.store_pos_department = sd.store_pos_department
            INNER JOIN item_pos_flags ipf(NOLOCK) ON ipf.item_id = im.item_id and ipf.pos_area = 1
            INNER JOIN vendor_master vm(NOLOCK) ON vm.v_id = vi.v_id and vm.vendor <> '88888888'
            INNER JOIN SMRV_POS_ITEM_AUTHORIZED spi(NOLOCK) ON
            spi.ITEM_ID = vi.item_id and spi.v_id = vi.v_id and spi.vi_id = vi.vi_id and spi.STORE_ID = sip.store_id
            INNER JOIN store_table st(NOLOCK) ON
            st.store_id = spi.STORE_ID and st.store_id = sip.store_id
            LEFT JOIN ITEM_SCALE_FLAGS(NOLOCK) isf
            ON isf.ITEM_ID = ipf.item_id
            WHERE
            dbo.SMR_TCI_UPCEAN('" + UPC + "') = im.UPC_EAN";

            SQL = SQL.Replace("\r\n", string.Empty);
            SqlCommand cmd = new SqlCommand(SQL, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            DataTable dt = new DataTable("MJR_PRICING");
            adapter.Fill(dt);

            if (dt != null && dt.Rows.Count > 0)
            {
                Price = float.Parse(dt.Rows[0]["Price"].ToString());
                UnitOM = dt.Rows[0]["UOM"].ToString().Trim() == "EA" ? UOM.EA : UOM.LB;
                Description = dt.Rows[0]["Description"].ToString().Trim();
            }
            else
            {
                Price = 0;
                Description = "nothing";

            }
        }
    }
}
