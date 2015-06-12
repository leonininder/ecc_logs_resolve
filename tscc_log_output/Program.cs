using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;


namespace tscc_log_output
{
    class Program
    {
        //main logs array
        private byte[] logs_array;
        //減掉search後的長度再=logs_array.
        private byte[] logs_array_temp;
        //最後的總交易數量
        private int trade_total = 0;
        //判斷解析中是否在search內找不到message type
        bool sw_str = true;
        //在output檔案中顯示 0x0000 格式
        private String hex_string;

        StreamWriter sw;

        static void Main(string[] args)
        {
            Program resolve = new Program();

            string[] filename_list = Directory.GetFiles((Environment.CurrentDirectory) + "\\logs\\");

            //take to logs file
            List<String> dir_list = new List<String>();
            foreach (String item in filename_list)
            {
                dir_list.Add(Path.GetFileName(item));
            }
            filename_list = dir_list.ToArray();

            //logs file each
            for (int i = 0; i < filename_list.Length; i++)
            {
                FileStream reader = new FileStream((System.Environment.CurrentDirectory) + "\\logs\\" + filename_list[i], FileMode.Open);
                resolve.logs_array = new byte[reader.Length];
                reader.Read(resolve.logs_array, 0, (int)reader.Length);
                reader.Close();

                // --- wirte file ---
                if (resolve.logs_array[1] == 255)
                {
                    resolve.sw = new StreamWriter((Environment.CurrentDirectory) + "\\output\\" + filename_list[i] + ".txt");

                    try
                    {
                        while (resolve.logs_array.Length != 0)
                        {
                            resolve.search_data_content();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    resolve.sw.Close();

                    if (resolve.sw_str == true)
                    {
                        Console.Write(filename_list[i] + "  解析結束  MAC 數量0 \n");
                        Console.Write("------------------------------------------------------------\n\n");
                    }
                    else 
                    {
                        Console.Write(filename_list[i] + "  解析不完全  MAC 數量0 \n");
                        Console.Write("------------------------------------------------------------\n\n");
                    }
                }
                else
                {
                    Console.Write(filename_list[i] + "  解析失敗! 不是正確的檔案格式 \n");
                    Console.Write("------------------------------------------------------------\n\n");
                }
            }

            Console.Write("所有Log交易總數量" + resolve.trade_total + "\n\n");
            Console.Write("Press any key to continue . . . ");

            Console.ReadKey(true);
        }

        private void search_data_content()
        {
            //用message type 去判斷目前的array在哪一種訊息, 判斷完寫入output再減去該訊息的長度, 然後再回來判斷一次, 直到arrary為0
            int search = (char)logs_array[1];
            switch (search)
            {
                case 255: //ok. if length over 54?
                    sw.WriteLine("--------------------------------Header--------------------------------");  // ecc_pdf p.28
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time                            :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("device_provider                      :" + logs_array[6]);
                    hex_string = byte_to_hex_string(7, 2, true);
                    sw.WriteLine("device_id                            :" + Convert.ToInt32(hex_string.Remove(0, 1), 16) + "    (0x" + hex_string + ")");
                    sw.WriteLine("service_provider                     :" + logs_array[9]);
                    sw.WriteLine("location_id                          :" + logs_array[10]);
                    sw.WriteLine("bus_type                             :" + logs_array[11]);
                    sw.WriteLine("bus_number                           :" + byte_to_unicode_merge(12, 10));
                    sw.WriteLine("set_number                           :" + logs_array[22]);
                    sw.WriteLine("des_key                              :" + logs_array[23]);
                    sw.WriteLine("MFRC                                 :" + byte_to_hex_string_to_hex(24, 4, true) );
                    sw.WriteLine("entries                              :" + logs_array[28]);
                    sw.WriteLine("A_F_01                               :" + (char)logs_array[29]);
                    sw.WriteLine("file_name_01                         :" + byte_to_unicode_merge(30, 12));
                    sw.WriteLine("A_F_02                               :" + (char)logs_array[42]);
                    sw.WriteLine("file_name_02                         :" + byte_to_unicode_merge(43, 12));
                    sw.WriteLine("close_flag                           :未彙整");  //?
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 55];    //length 54 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 55, logs_array_temp, 0, logs_array.Length - 55);
                    logs_array = logs_array_temp;
                    break;

                case 12:    //mac != 0?
                    sw.WriteLine("------------------------------加值重送--------------------------------");  // ecc_pdf p.29
                    sw.WriteLine("length_of_record                     :" + logs_array[0]); 
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("sub_type                             :" + logs_array[2]);
                    sw.WriteLine("transaction_date_time   交易日期時間 :" + byte_convert_unix_date_time(3, 4, true));
                    sw.WriteLine("card_physical_id        卡片晶片號碼 :" + byte_to_hex_string_to_hex(7, 4, true));
                    sw.WriteLine("issuer_code             發卡單位代碼 :" + logs_array[11]);
                    sw.WriteLine("tran_SEQNUM                 交易序號 :" + logs_array[12]);
                    sw.WriteLine("transaction_amount          交易金額 :" + byte_to_hex_string_to_hex(13, 2, true));
                    sw.WriteLine("Electronic_Remaining_value  卡片餘額 :" + byte_to_hex_string_to_hex(15, 2, true));
                    sw.WriteLine("loyalty_counter             忠誠點數 :" + byte_to_hex_string_to_hex(17, 2, true));
                    sw.WriteLine("bank_code                   銀行代碼 :" + logs_array[19]);
                    hex_string = byte_to_hex_string(20, 4, true);
                    sw.WriteLine("Transaction_equipment   交易設備編號 :" + "0x" + hex_string );
                    sw.WriteLine("set_number                           :" + logs_array[24]);
                    sw.WriteLine("des_key                              :" + logs_array[25]);
                    sw.WriteLine("MAC                                  :" + byte_to_hex_string_to_hex(26, 4, true));    //-------------------
                    sw.WriteLine("MFRC                                 :" + byte_to_hex_string_to_hex(30, 4, true));
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 34];    //length 33 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 34, logs_array_temp, 0, logs_array.Length - 34);
                    logs_array = logs_array_temp;
                    break;

                case 29:    //new
                    sw.WriteLine("---------------------------卡片禁用記錄-------------------------------");  // ecc_pdf p.30
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("transaction_date_time   交易日期時間 :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("card_physical_id        卡片晶片號碼 :" + byte_to_hex_string_to_hex(6, 4, true));
                    sw.WriteLine("Refusal_code            被禁用的代碼 :" + logs_array[10]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 11];    //length 10 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 11, logs_array_temp, 0, logs_array.Length - 11);
                    logs_array = logs_array_temp;
                    break;

                case 34:    //new
                    sw.WriteLine("----------------------------攔截黑名單--------------------------------");  // ecc_pdf p.30
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("Personal_Profile                票種 :" + logs_array[2]);
                    sw.WriteLine("transaction_date_time   交易日期時間 :" + byte_convert_unix_date_time(3, 4, true));
                    sw.WriteLine("card_physical_id        卡片晶片號碼 :" + byte_to_hex_string_to_hex(7, 4, true));
                    sw.WriteLine("issuer_code             發卡單位代碼 :" + logs_array[11]);
                    sw.WriteLine("Blocking_Reason         鎖卡原因代碼 :" + logs_array[12]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 13];    //length 12 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 13, logs_array_temp, 0, logs_array.Length - 13);
                    logs_array = logs_array_temp;
                    break;

                case 42:    //最後面有錯
                    sw.WriteLine("---------------------------AR班次營收統計-----------------------------");  // ecc_pdf p.30
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time                            :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("普通卡交易總筆數                     :" + byte_to_hex_string_to_hex(6, 2, true));
                    sw.WriteLine("普通卡交易總金額                     :" + byte_to_hex_string_to_hex(8, 4, true));
                    sw.WriteLine("敬老1卡交易總筆數                    :" + byte_to_hex_string_to_hex(12, 2, true));
                    sw.WriteLine("敬老1卡交易總金額                    :" + byte_to_hex_string_to_hex(14, 4, true));
                    sw.WriteLine("敬老2卡交易總筆數                    :" + byte_to_hex_string_to_hex(18, 2, true));
                    sw.WriteLine("敬老2卡交易總金額                    :" + byte_to_hex_string_to_hex(20, 4, true));
                    sw.WriteLine("愛心卡交易總筆數                     :" + byte_to_hex_string_to_hex(24, 2, true));
                    sw.WriteLine("愛心卡交易總金額                     :" + byte_to_hex_string_to_hex(26, 4, true));
                    sw.WriteLine("陪伴卡交易總筆數                     :" + byte_to_hex_string_to_hex(30, 2, true));
                    sw.WriteLine("陪伴卡交易總金額                     :" + byte_to_hex_string_to_hex(32, 4, true));
                    sw.WriteLine("舊學生卡交易總筆數                   :" + byte_to_hex_string_to_hex(36, 2, true));
                    sw.WriteLine("舊學生卡交易總金額                   :" + byte_to_hex_string_to_hex(38, 4, true));
                    sw.WriteLine("新學生卡交易總筆數                   :" + byte_to_hex_string_to_hex(42, 2, true));
                    sw.WriteLine("新學生卡交易總金額                   :" + byte_to_hex_string_to_hex(44, 4, true));
                    sw.WriteLine("警察卡交易總筆數                     :" + byte_to_hex_string_to_hex(48, 2, true));
                    sw.WriteLine("警察卡交易總金額                     :" + byte_to_hex_string_to_hex(50, 4, true));
                    sw.WriteLine("優待卡交易總筆數                     :" + byte_to_hex_string_to_hex(54, 2, true));
                    sw.WriteLine("優待卡交易總金額                     :" + byte_to_hex_string_to_hex(56, 4, true));
                    sw.WriteLine("保留卡1交易總筆數                    :" + byte_to_hex_string_to_hex(60, 2, true));
                    sw.WriteLine("保留卡1交易總金額                    :" + byte_to_hex_string_to_hex(62, 4, true));
                    sw.WriteLine("保留卡2交易總筆數                    :" + byte_to_hex_string_to_hex(66, 2, true));
                    sw.WriteLine("保留卡2交易總金額                    :" + byte_to_hex_string_to_hex(68, 4, true));
                    sw.WriteLine("保留卡3交易總筆數                    :" + byte_to_hex_string_to_hex(72, 2, true));
                    sw.WriteLine("保留卡3交易總金額                    :" + byte_to_hex_string_to_hex(74, 4, true));
                    sw.WriteLine("保留卡4交易總筆數                    :" + byte_to_hex_string_to_hex(78, 2, true));
                    sw.WriteLine("保留卡4交易總金額                    :" + byte_to_hex_string_to_hex(80, 4, true));
                    sw.WriteLine("保留卡5交易總筆數                    :" + byte_to_hex_string_to_hex(84, 2, true));
                    sw.WriteLine("保留卡5交易總金額                    :" + byte_to_hex_string_to_hex(86, 4, true));
                    sw.WriteLine("保留卡6交易總筆數                    :" + byte_to_hex_string_to_hex(90, 2, true));
                    sw.WriteLine("保留卡6交易總金額                    :" + byte_to_hex_string_to_hex(92, 4, true));
                    sw.WriteLine("保留卡7交易總筆數                    :" + byte_to_hex_string_to_hex(96, 2, true));
                    sw.WriteLine("保留卡7交易總金額                    :" + byte_to_hex_string_to_hex(98, 4, true));

                    //原文件有錯 p.31最後一行
                    sw.WriteLine("總減值筆數                           :" + byte_to_hex_string_to_hex(102, 2, true));
                    sw.WriteLine("總減值金額                           :" + byte_to_hex_string_to_hex(104, 4, true));
                    sw.WriteLine("總轉乘優惠筆數                       :" + byte_to_hex_string_to_hex(108, 2, true));
                    sw.WriteLine("總轉乘優惠金額                       :" + byte_to_hex_string_to_hex(110, 4, true));
                    sw.WriteLine("總自動加值筆數                       :" + byte_to_hex_string_to_hex(114, 2, true));
                    sw.WriteLine("總自動加值金額                       :" + byte_to_hex_string_to_hex(116, 4, true));
                    sw.WriteLine("特種票種類數量(未用)                 :" + byte_to_hex_string_to_hex(120, 2, true));
                    sw.WriteLine("下列特舉特種票數量                   :" + byte_to_hex_string_to_hex(122, 1, true));
                    sw.WriteLine("特種票 1 總筆數                      :" + byte_to_hex_string_to_hex(123, 2, true));
                    //1
                    //2

                    logs_array_temp = new byte[logs_array.Length - 153];    //length 152 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 153, logs_array_temp, 0, logs_array.Length - 153);
                    logs_array = logs_array_temp;
                    break;

                case 71:    //new
                    sw.WriteLine("--------------------------------開班----------------------------------");  // ecc_pdf p.32
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_start_of_shift 開班日期時間:" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("Type_of_shift                開班種類:" + logs_array[6]);
                    sw.WriteLine("Shift_number                 開班代碼:" + byte_to_hex_string_to_hex(7, 2, true));
                    sw.WriteLine("Agent_number                 司機代碼:" + byte_to_hex_string_to_hex(9, 2, true));
                    sw.WriteLine("Line_number                  路線代碼:" + byte_to_hex_string_to_hex(11, 2, true));
                    sw.WriteLine("Depot_code                   場站代碼:" + logs_array[13]);
                    sw.WriteLine("Line_group                   路線群組:" + logs_array[14]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 15];  //length 14 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 15, logs_array_temp, 0, logs_array.Length - 15);
                    logs_array = logs_array_temp;
                    break;

                case 72:    //new
                    sw.WriteLine("--------------------------------收班----------------------------------");  // ecc_pdf p.33
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_end_of_shift   收班日期時間:" + byte_convert_unix_date_time(2, 4, true) );
                    sw.WriteLine("Shift_number                 開班代碼:" + byte_to_hex_string_to_hex(6, 2, true) );
                    sw.WriteLine("Agent_number                 司機代碼:" + byte_to_hex_string_to_hex(8, 2, true) );
                    sw.WriteLine("Line_number                  路線代碼:" + byte_to_hex_string_to_hex(10, 2, true) );
                    sw.WriteLine("Depot_code                   場站代碼:" + logs_array[12]);
                    sw.WriteLine("Line_group                   路線群組:" + logs_array[13]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 14];  //length 13 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 14, logs_array_temp, 0, logs_array.Length - 14);
                    logs_array = logs_array_temp;
                    break;

                case 73:    //new
                    sw.WriteLine("---------------------------進退段區段修改-----------------------------");  // ecc_pdf p.33
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_of_the_event   事件日期時間:" + byte_convert_unix_date_time(2, 4, true) );
                    sw.WriteLine("zone_number_first_of_bz  進退後的區段:" + logs_array[6]);
                    sw.WriteLine("2nd_zone_of_bz         進退後的緩衝區:" + logs_array[7]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 8];  //length 7 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 8, logs_array_temp, 0, logs_array.Length - 8);
                    logs_array = logs_array_temp;
                    break;

                case 74:    //new
                    sw.WriteLine("------------------------------路線修改--------------------------------");  // ecc_pdf p.34
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_of_the_event   事件日期時間:" + byte_convert_unix_date_time(2, 4, true) );
                    sw.WriteLine("Line_number                  路線代碼:" + byte_to_hex_string_to_hex(6, 2, true) );
                    sw.WriteLine("Bus_line_group           公車轉乘群組:" + logs_array[8]);
                    sw.WriteLine("Number_of_zones          設定區段數量:" + logs_array[9]);
                    sw.WriteLine("Buffer_zone_setting  設定是否有緩衝區:" + logs_array[10]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 11];  //length 10 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 11, logs_array_temp, 0, logs_array.Length - 11);
                    logs_array = logs_array_temp;
                    break;

                case 81:    //ok
                    sw.WriteLine("-------------------------------開關機---------------------------------");  // ecc_pdf p.34
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_of_the_event   事件日期時間:" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("time_of_power_off    末次電源關閉時間:" + byte_convert_unix_date_time(6, 4, true));

                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 10];  //length 9 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 10, logs_array_temp, 0, logs_array.Length - 10);
                    logs_array = logs_array_temp;
                    break;

                case 86:    //ok
                    sw.WriteLine("------------------------------校正時間--------------------------------");  // ecc_pdf p.34???
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("old_date_time                        :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("new_date_time                        :" + byte_convert_unix_date_time(6, 4, true));

                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 10];  //length 9 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 10, logs_array_temp, 0, logs_array.Length - 10);
                    logs_array = logs_array_temp;
                    break;

                case 110:   //ok
                    //進出旗標 (上車21  下車20)
                    if (logs_array[21] == 21)
                    {
                        sw.WriteLine("-----------------------V2里程普通卡上車扣款---------------------------");  // ecc_pdf p.35
                    }
                    else 
                    {
                        sw.WriteLine("-----------------------V2里程普通卡下車扣款---------------------------");  // ecc_pdf p.35
                    }

                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("sub_type                        票種 :" + logs_array[2]);
                    sw.WriteLine("transaction_date_time   交易日期時間 :" + byte_convert_unix_date_time(3, 4, true));
                    sw.WriteLine("card_physical_id        卡片晶片號碼 :" + byte_to_hex_string_to_hex(7, 4, true));   //------------------------------
                    sw.WriteLine("issuer_code             發卡單位代碼 :" + logs_array[11]);
                    sw.WriteLine("tran_SEQNUM                 交易序號 :" + logs_array[12]);
                    sw.WriteLine("transaction_amount    交易金額(實扣) :" + byte_to_hex_string_to_hex(13, 2, true));
                    sw.WriteLine("electronic_remaining_value  卡片餘額 :" + byte_to_hex_string_to_hex(15, 2, true));
                    sw.WriteLine("line_number                 路線代碼 :" + byte_to_hex_string_to_hex(17, 2, true));
                    sw.WriteLine("current_zone_number         目前區段 :" + logs_array[19] + "   (下車計費站號)");
                    hex_string = byte_to_hex_string(20, 1, true);
                    sw.WriteLine("transfer_group_code     轉乘群組代碼 :0x" + hex_string + " (前次 3  本次 3)");
                    sw.WriteLine("entry_exit_flag             進出旗標 :" + logs_array[21] + "  (上車21  下車20)");
                    sw.WriteLine("entry_zone_number           進站號碼 :" + logs_array[22] + "   (上車計費站號)");
                    sw.WriteLine("owner_area_code           卡片區域碼 :" + logs_array[23]);
                    sw.WriteLine("transfer_discount           轉乘優惠 :" + byte_to_hex_string_to_hex(24, 2, true));
                    sw.WriteLine("personal_discount           個人優惠 :" + byte_to_hex_string_to_hex(26, 2, true));
                    sw.WriteLine("loyalty_counter             忠誠點數 :" + byte_to_hex_string_to_hex(28, 2, true));
                    sw.WriteLine("up_tran_SEQNUM          上次交易序號 :" + logs_array[30] + "  (上車:放本次上車交易序號  下車:放上車交易序號)");
                    sw.WriteLine("transaction_advance_amount    預扣款 :" + byte_to_hex_string_to_hex(31, 2, true) + "   (上車時為0  下車時為上車實扣款(要加上點數優惠轉換的金額))");
                    sw.WriteLine("ORI_AMT                 原價(未打折) :" + byte_to_hex_string_to_hex(33, 2, true));
                    sw.WriteLine("mileage_tran_flag   里程轉乘優惠記錄 :" + logs_array[35] + "   (0:本次交易未符合轉乘優惠資格   1:本次交易符合轉乘優惠資格)");
                    sw.WriteLine("Other_DISC              其他輔助金額 :" + byte_to_hex_string_to_hex(36, 2, true));
                    sw.WriteLine("up_tran_time   公車轉乘-上筆交易時間 :" + byte_convert_unix_date_time(38, 4, true));
                    hex_string = byte_to_hex_string(42, 4, true);
                    sw.WriteLine("up_tran_DEVID  公車轉乘-上筆設備編號 :" + byte_to_hex_string_to_hex(42, 4, true) + " (0x" + hex_string + ")   拆解後資訊->業者代碼:33  設備種類:3  設備ID:3862");
                    sw.WriteLine("up_tran_SPID   公車轉乘-上筆業者代碼 :" + logs_array[46]);
                    sw.WriteLine("RFU                                  :" + byte_to_hex_string_to_hex(47, 2, true) + "   (定值0x00)");
                    sw.WriteLine("set_number                           :" + logs_array[49] + "   (定值0x00)");
                    sw.WriteLine("des_key                              :" + logs_array[50] + "   (定值0x01)");
                    hex_string = byte_to_hex_string(51, 4, true);
                    sw.WriteLine("transMAC                             :" + byte_to_hex_string_to_hex(51, 4, true) + "  (0x" + hex_string + ")");    // +  (0xB83A62BD)------------------------
                    sw.WriteLine("MFRC                                 :" + byte_to_hex_string_to_hex(55, 4, true));
                    sw.WriteLine("Driver_No                   司機代碼 :" + byte_to_hex_string_to_hex(59, 2, true));
                    sw.WriteLine("Shift_Time                  開班時間 :" + byte_convert_unix_date_time(61, 4, true));
                    sw.WriteLine("Shift_No                        班別 :" + byte_to_hex_string_to_hex(65, 2, true));
                    sw.WriteLine("TXN_Personal_Profile      交易身分別 :" + logs_array[67]);
                    sw.WriteLine("EV_BEF_TXN            交易前卡片金額 :" + byte_to_hex_string_to_hex(68, 2, true));
                    sw.WriteLine("Dis_Rate                        費率 :" + logs_array[70]);
                    sw.WriteLine("Ticket_AMT              票價(打折後) :" + byte_to_hex_string_to_hex(71, 2, true));
                    sw.WriteLine("Peak_Disc                   尖峰優惠 :" + byte_to_hex_string_to_hex(73, 2, true));
                    sw.WriteLine("Penalty                         罰款 :" + byte_to_hex_string_to_hex(75, 2, true));
                    sw.WriteLine("Personal_Use_Points   輔助款使用點數 :" + byte_to_hex_string_to_hex(77, 2, true));
                    sw.WriteLine("Personal_Counter      輔助款累積點數 :" + logs_array[79]);
                    sw.WriteLine("up_tran_SEQ_NO 公車轉乘-上筆交易序號 :" + byte_to_hex_string_to_hex(80, 3, true));
                    sw.WriteLine("Card_Type                       票別 :" + logs_array[83]);
                    sw.WriteLine("RFU1                        保留欄位 :" + rfu_format(84, 12));
                    sw.WriteLine("RFU1_VER                    欄位版本 :" + logs_array[96]);
                    sw.WriteLine("Go_Return_Flag            往返程註記 :" + logs_array[97] + "   (去程1  返程2  循環3)");
                    sw.WriteLine("Current_Zone_Number2    目前招呼站號 :" + byte_to_hex_string_to_hex(98, 2) + "   (上車時等於進站招呼站號  下車時等於下車招呼站號)");    //-----------------
                    sw.WriteLine("RFU2_3                          保留 :" + byte_to_hex_string_to_hex(100, 2, true));
                    sw.WriteLine("Other_Disc2            其他輔助金額2 :" + byte_to_hex_string_to_hex(102, 2, true) + "   (下車60元上限輔助之類的)");
                    sw.WriteLine("Other_Disc3            其他輔助金額3 :" + byte_to_hex_string_to_hex(104, 2, true));
                    sw.WriteLine("RFU2_5                      保留欄位 :" + rfu_format(106, 7) );
                    sw.WriteLine("RFU2_VER              欄位版本(業者) :" + logs_array[113] + "   (此為第一版  固定填入1)");
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 114];    //length 113 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 114, logs_array_temp, 0, logs_array.Length - 114);
                    logs_array = logs_array_temp;

                    trade_total++;
                    break;

                case 120:   //new
                    sw.WriteLine("----------------------------特種票-鎖卡-------------------------------");  // ecc_pdf p.38
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("Fare_product_type          特種票種類:" + logs_array[2]);
                    sw.WriteLine("transaction_date_time    交易日期時間:" + byte_convert_unix_date_time(3, 4, true));
                    sw.WriteLine("card_physical_id         卡片晶片號碼:" + byte_to_hex_string_to_hex(7, 4, true));
                    sw.WriteLine("issuer_code              發卡單位代碼:" + logs_array[11]);
                    sw.WriteLine("Blocking_Reason          鎖卡原因代碼:" + logs_array[12]);
                    sw.WriteLine("transaction_number           交易序號:" + byte_to_hex_string_to_hex(13, 2, true));
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 15];  //length 14 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 15, logs_array_temp, 0, logs_array.Length - 15);
                    logs_array = logs_array_temp;
                    break;

                case 121:   //new
                    sw.WriteLine("------------------------------折返作業--------------------------------");  // ecc_pdf p.38
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_start_of_shift 折返日期時間:" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("Line_number                  路線代碼:" + byte_to_hex_string_to_hex(6, 2, true) );
                    sw.WriteLine("Agent_number                 司機代碼:" + byte_to_hex_string_to_hex(8, 2, true) );
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 10];  //length 9 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 10, logs_array_temp, 0, logs_array.Length - 10);
                    logs_array = logs_array_temp;
                    break;

                case 122:   //ok
                    sw.WriteLine("------------------------------里程GPS換站-----------------------------");  // ecc_pdf p.38
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_of_start_of_shift          :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("line_number                          :" + byte_to_hex_string_to_hex(6, 2, true));
                    sw.WriteLine("agent_number                         :" + byte_to_hex_string_to_hex(8, 2, true));
                    sw.WriteLine("up_zone_number                       :" + byte_to_hex_string_to_hex(10, 2, true));
                    sw.WriteLine("current_zone_number                  :" + byte_to_hex_string_to_hex(12, 2, true));
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 14];  //length 13 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 14, logs_array_temp, 0, logs_array.Length - 14);
                    logs_array = logs_array_temp;
                    break;

                case 123:   //ok
                    sw.WriteLine("---------------------------里程手動換站-------------------------------");  // ecc_pdf p.39
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_of_start_of_shift          :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("line_number                          :" + byte_to_hex_string_to_hex(6, 2, true));
                    sw.WriteLine("agent_number                         :" + byte_to_hex_string_to_hex(8, 2, true));
                    sw.WriteLine("up_zone_number                       :" + byte_to_hex_string_to_hex(10, 2, true));
                    sw.WriteLine("current_zone_number                  :" + byte_to_hex_string_to_hex(12, 2, true));
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 14];  //length 13 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 14, logs_array_temp, 0, logs_array.Length - 14);
                    logs_array = logs_array_temp;
                    break;

                case 124:   //new
                    sw.WriteLine("------------------------------逃票事件--------------------------------");  // ecc_pdf p.39
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("transaction_date_time    交易日期時間:" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("card_physical_id         卡片晶片號碼:" + byte_to_hex_string_to_hex(6, 4, true));
                    sw.WriteLine("UP_transaction_date_time 上次交易時間:" + byte_convert_unix_date_time(10, 4, true));
                    sw.WriteLine("UP_Transaction_deviceID 上次交易devID:" + byte_to_hex_string_to_hex(14, 4, true));
                    sw.WriteLine("UP_transaction_SP    上次交易業者代碼:" + logs_array[18]);
                    sw.WriteLine("UP_line_number       上次交易路線代號:" + byte_to_hex_string_to_hex(19, 2, true));
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 21];  //length 20 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 21, logs_array_temp, 0, logs_array.Length - 21);
                    logs_array = logs_array_temp;
                    break;

                case 130:   //ok
                    sw.WriteLine("-----------------------------里程手動開班-----------------------------");  // ecc_pdf p.40
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_start_of_shift             :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("type_of_shift                        :" + logs_array[6]);
                    sw.WriteLine("shift_number                         :" + byte_to_hex_string_to_hex(7, 2, true) );
                    sw.WriteLine("agent_number                         :" + byte_to_hex_string_to_hex(9, 2, true));
                    sw.WriteLine("line_number                          :" + byte_to_hex_string_to_hex(11, 2, true));
                    sw.WriteLine("depot_code                           :" + logs_array[13]);
                    sw.WriteLine("line_group                           :" + logs_array[14]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 15];  //length 14 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 15, logs_array_temp, 0, logs_array.Length - 15);
                    logs_array = logs_array_temp;
                    break;

                case 131:   //ok
                    sw.WriteLine("-----------------------------里程手動結班-----------------------------");  // ecc_pdf p.40
                    sw.WriteLine("length_of_record                     :" + logs_array[0]);
                    sw.WriteLine("message_type                         :" + logs_array[1]);
                    sw.WriteLine("date_time_end_of_shift               :" + byte_convert_unix_date_time(2, 4, true));
                    sw.WriteLine("shift_number                         :" + byte_to_hex_string_to_hex(6, 2, true));//2
                    sw.WriteLine("agent_number                         :" + byte_to_hex_string_to_hex(8, 2, true));//2
                    sw.WriteLine("line_number                          :" + byte_to_hex_string_to_hex(10, 2, true));//2
                    sw.WriteLine("depot_code                           :" + logs_array[12]);
                    sw.WriteLine("line_group                           :" + logs_array[13]);
                    sw.WriteLine("----------------------------------------------------------------------");

                    logs_array_temp = new byte[logs_array.Length - 14];  //length 13 + 紀錄length的 1 byte
                    Array.Copy(logs_array, 14, logs_array_temp, 0, logs_array.Length - 14);
                    logs_array = logs_array_temp;
                    break;

                default:
                    sw_str = false;
                    logs_array = new byte[0];
                    break;
            }
        }


        //utility
        private String byte_to_unicode_merge(int index, int num)
        {
            byte[] logs_array_temp = new byte[num];
            Array.Copy(logs_array, index, logs_array_temp, 0, num);

            String string_merge = "";

            for (int temp = 0; temp < num - 1; temp++)
            {
                string_merge += (char)logs_array_temp[temp];
            }

            return string_merge;
        }

        private String byte_convert_unix_date_time(int index, int num, bool LSB = false)
        {

            logs_array_temp = new byte[num];
            Array.Copy(logs_array, index, logs_array_temp, 0, num);

            if (LSB) { logs_array_temp = change_hlbyte(logs_array_temp); }
            String rltStr = "";
            for (int i = 0; i < logs_array_temp.Length; i++)
            {
                rltStr += Convert.ToString(logs_array_temp[i], 16).PadLeft(2, '0');
            }
            long unixTime = Convert.ToInt64(rltStr, 16);

            DateTime origin1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin1 = origin1.AddSeconds(unixTime);

            return origin1.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private byte[] change_hlbyte(byte[] byte_temp)
        {
            byte[] byte_convert = new byte[byte_temp.Length];

            int j = 0;
            for (int i = byte_temp.Length - 1; i >= 0; i--)
            {
                byte_convert[j] = byte_temp[i];
                j++;
            }

            return byte_convert;
        }

        private String byte_to_hex_string(int index, int num, bool LSB = false)
        {
            logs_array_temp = new byte[num];
            Array.Copy(logs_array, index, logs_array_temp, 0, num);

            if (LSB) { logs_array_temp = change_hlbyte(logs_array_temp); }

            String hex_string = string.Empty;
            if (logs_array_temp != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < logs_array_temp.Length; i++)
                {
                    strB.Append(logs_array_temp[i].ToString("X2"));
                }
                hex_string = strB.ToString();
            }
            return hex_string;
        }

        private uint byte_to_hex_string_to_hex(int index, int num, bool LSB = false) //e.g. AAAA to 0xAAAA
        {
            logs_array_temp = new byte[num];
            Array.Copy(logs_array, index, logs_array_temp, 0, num);

            if (LSB) { logs_array_temp = change_hlbyte(logs_array_temp); }

            String hex_string = string.Empty;
            if (logs_array_temp != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < logs_array_temp.Length; i++)
                {
                    strB.Append(logs_array_temp[i].ToString("X2"));
                }
                hex_string = strB.ToString();
            }

            return Convert.ToUInt32(hex_string, 16);
        }

        //保留欄位的格式
        private String rfu_format(int index, int num, bool LSB = false) //e.g. 0 0 0 0 31 35
        {
            logs_array_temp = new byte[num];
            Array.Copy(logs_array, index, logs_array_temp, 0, num);

            if (LSB) { logs_array_temp = change_hlbyte(logs_array_temp); }

            String string_format = "";
            if (logs_array_temp != null)
            {
                for (int i = 0; i < logs_array_temp.Length; i++)
                {
                    string_format += logs_array_temp[i] + " ";
                }
            }

            return string_format;
        }
    } 
}
