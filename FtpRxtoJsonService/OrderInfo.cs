namespace FtpRxtoJsonService
{
    public class Dprovider
    {
        public int x_dprovider_account { get; set; }
        public string x_dprovider_name { get; set; }
        public string x_dprovider_telephone { get; set; }
        public string x_dprovider_taxid { get; set; }
        public string x_dprovider_address1 { get; set; }
        public string x_dprovider_address2 { get; set; }
        public string x_dprovider_city { get; set; }
        public int x_dprovider_zip { get; set; }
        public string x_dprovider_state { get; set; }
        public string x_dprovider_county { get; set; }
        public string x_dprovider_country { get; set; }
    }

    public class Frame
    {
        public string frame_tracing { get; set; }
        public string frame_edge { get; set; }
        public string frame_upc { get; set; }
        public string frame_vendor { get; set; }
        public string frame_model { get; set; }
        public int frame_color { get; set; }
        public int frame_eye { get; set; }
        public int frame_bridge { get; set; }
        public int frame_temple { get; set; }
        public string frame_status { get; set; }
        public double frame_a { get; set; }
        public double frame_b { get; set; }
        public double frame_dbl { get; set; }
        public double frame_ed { get; set; }
        public double frame_circ { get; set; }
        public double frame_long_rad { get; set; }
        public double frame_rad_angle { get; set; }
        public string frame_mounting { get; set; }
    }

    public class Item
    {
        public string sku { get; set; }
        public string item_source { get; set; }
        public string item_side { get; set; }
        public string item_part_rx { get; set; }
        public int item_quantity { get; set; }
        public string item_description { get; set; }
    }

    public class LensOd
    {
        public int x_lens_od_style_code { get; set; }
        public int x_lens_od_material_code { get; set; }
        public int x_lens_od_color_code { get; set; }
        public string x_lens_od_material_desc { get; set; }
        public string x_lens_od_style_desc { get; set; }
        public string x_lens_od_color_desc { get; set; }
    }

    public class LensOs
    {
        public int x_lens_os_style_code { get; set; }
        public int x_lens_os_material_code { get; set; }
        public int x_lens_os_color_code { get; set; }
        public string x_lens_os_material_desc { get; set; }
        public string x_lens_os_style_desc { get; set; }
        public string x_lens_os_color_desc { get; set; }
    }

    public class Order
    {
        public int order_id { get; set; }
        public string agent_version { get; set; }
        public string agent_name { get; set; }
        public int lab_num { get; set; }
        public int cust_num { get; set; }
        public string cust_seq_num { get; set; } 
        public string customer_po_num { get; set; }
        public int customer_tray_num { get; set; }
        public int Customer_tray { get; set; }
        public int x_tray_num { get; set; }
        public DateTime date_ordered { get; set; } //revisar formato para saber si transformarlo a string o datetime  12-30-11' to 12/30/2011.  
        public string dr_name { get; set; }
        public string x_doc_no { get; set; }
        public int x_bill_cust_no { get; set; }
        public int x_ship_cust_no { get; set; }
        public string x_invoice_only { get; set; }
        public string x_rem_operator { get; set; }
        public string x_inno_user { get; set; }
        public int x_rx_oc_height_qual { get; set; }
        public int x_rx_seg_height_qual { get; set; }
        public int rx_eye { get; set; }
        public Dprovider dprovider { get; set; }
        public Frame frame { get; set; }
        public LensOd lens_od { get; set; }
        public LensOs lens_os { get; set; }
        public RxOd rx_od { get; set; }
        public RxOs rx_os { get; set; }
        public List<Item> items { get; set; }
    }

    public class Root
    {
        public Order order { get; set; }
    }

    public class RxOd
    {
        public double rx_od_sphere { get; set; }
        public double rx_od_cylinder { get; set; }
        public int rx_od_axis { get; set; }
        public double rx_od_far { get; set; }
        public double rx_od_near { get; set; }
        public double rx_od_add { get; set; }
        public double rx_od_seg_height { get; set; }
        public double rx_od_prism { get; set; }
        public string rx_od_prism_dir { get; set; }
        public int rx_od_prism_angle { get; set; }
    }

    public class RxOs
    {
        public double rx_os_sphere { get; set; }
        public double rx_os_cylinder { get; set; }
        public int rx_os_axis { get; set; }
        public double rx_os_far { get; set; }
        public double rx_os_near { get; set; }
        public double rx_os_add { get; set; }
        public double rx_os_seg_height { get; set; }
        public double rx_os_prism { get; set; }
        public string rx_os_prism_dir { get; set; }
        public int rx_os_prism_angle { get; set; }
    }




}


