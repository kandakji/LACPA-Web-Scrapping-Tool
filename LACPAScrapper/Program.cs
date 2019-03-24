using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Drawing;

namespace LACPAScrapper
{
    public class Program
    {
        static int SaveLocalizedText(SqlConnection connection, string text)
        {
            int result = 0;

            SqlCommand command = connection.CreateCommand();
            command.CommandText = "insert into LocalizedTextID (EnglishText) values (@Text)";
            command.Parameters.AddWithValue("@Text", text);
            command.ExecuteNonQuery();

            command.CommandText = "select @@identity";
            command.Parameters.Clear();

            result = int.Parse(command.ExecuteScalar().ToString());

            return result;
        }

        static StreamWriter writer = new StreamWriter("log.txt");

        static void Main(string[] args)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();

                    command.CommandText = @"truncate table Members;
                                            truncate table AccountancyFirms";

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();







                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        
        


            int j = 1;
            string x = NumberofPages("http://www.grafium.solutions/dev/directory/trainees");
            Console.WriteLine("Extracting Trainees...");
            ExtractTrainees(Convert.ToInt32(x), ref j);
            x = NumberofPages("http://www.grafium.solutions/dev/directory/practicing-members");
            Console.WriteLine("Extracting Practicing Members...");
            ExtractPracticingMembers(Convert.ToInt32(x), ref j);
            x = NumberofPages("http://grafium.solutions/dev/directory/accountancy-firm");
            Console.WriteLine("Extracting Accountancy Firm...");
            ExtractAccountancyFirms(Convert.ToInt32(x), ref j);
            writer.Close();
        }

        static void ExtractTrainees(int n, ref int j)
        {
            int Mtype = 1;

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    connection.Open();
                    for (int i = 0; i <= n; i++)
                    {
                        SqlCommand command = connection.CreateCommand();

                        string url = "http://grafium.solutions/dev/directory/trainees?page=" + Convert.ToString(i); ;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string PageHtml = reader.ReadToEnd();
                        reader.Close();

                        Regex regex = new Regex("<div class=\"views-field views-field-field-number\">        <div class=\"field-content\">(.+?)</div>  </div>  </div>", RegexOptions.Multiline);
                        //Regex regexlp = new Regex("<a title=\"Go to last page\" class=\"btn btn-sm btn-default\" href=\"/dev/directory/trainees?page=(.+?)\">last", RegexOptions.Multiline);

                        //MatchCollection lpMatch = regexlp.Matches(PageHtml);
                        MatchCollection theMatches = regex.Matches(PageHtml);

                        foreach (Match m in theMatches)
                        {
                            Console.WriteLine("------");

                            string SubUrl = "http://www.grafium.solutions/dev/users/" + m.Groups[1].Value;

                            HttpWebRequest subrequest = (HttpWebRequest)WebRequest.Create(SubUrl);
                            HttpWebResponse subresponse = (HttpWebResponse)subrequest.GetResponse();

                            StreamReader subreader = new StreamReader(subresponse.GetResponseStream());
                            string SubPagehtml = subreader.ReadToEnd();
                            reader.Close();

                            command.CommandText = @"insert into Members (  Id
                                                                                  ,Name
                                                                                  ,MiddleName
                                                                                  ,Family
                                                                                  ,Title
                                                                                  ,FirmName
                                                                                  ,Mouhafaza
                                                                                  ,District
                                                                                  ,City
                                                                                  ,Street
                                                                                  ,Bldg
                                                                                  ,Floor
                                                                                  ,POBox
                                                                                  ,OfficePhone
                                                                                  ,OfficeFax
                                                                                  ,HomePhone
                                                                                  ,PersonalMobile
                                                                                  ,Email
                                                                                  ,MembershipTypeId
                                                                                  ,Url) values (   @Id
                                                                                                      ,@Name
                                                                                                      ,@Father
                                                                                                      ,@Family
                                                                                                      ,@Title
                                                                                                      ,@FirmName
                                                                                                      ,@Mouhafaza
                                                                                                      ,@District
                                                                                                      ,@City
                                                                                                      ,@Street
                                                                                                      ,@Bldg
                                                                                                      ,@Floor
                                                                                                      ,@POBox
                                                                                                      ,@OfficePhone
                                                                                                      ,@OfficeFax
                                                                                                      ,@HomePhone
                                                                                                      ,@PersonalMobile
                                                                                                      ,@Email
                                                                                                      ,@MembershipTypeId
                                                                                                      ,@Url)";
                            command.Parameters.Clear();

                            
                            command.Parameters.AddWithValue("@Id",j);
                            j++;
                            

                            Regex SubRegex = new Regex("<div class=\"views-field views-field-field-name\">    <span class=\"views-label views-label-field-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            MatchCollection theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Name", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Name", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-father\">    <span class=\"views-label views-label-field-father\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Father", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Father", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-family\">    <span class=\"views-label views-label-field-family\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Family", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Family", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-title\">    <span class=\"views-label views-label-field-title\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Title", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Title", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-firm-name\">    <span class=\"views-label views-label-field-firm-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@FirmName", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@FirmName", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-mouhafaza\">    <span class=\"views-label views-label-field-mouhafaza\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Mouhafaza", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Mouhafaza", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-district\">    <span class=\"views-label views-label-field-district\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@District", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@District", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-city\">    <span class=\"views-label views-label-field-city\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@City", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@City", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-street\">    <span class=\"views-label views-label-field-street\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Street", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Street", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-bldg\">    <span class=\"views-label views-label-field-bldg\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Bldg", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Bldg", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-floor\">    <span class=\"views-label views-label-field-floor\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Floor", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Floor", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-pobox\">    <span class=\"views-label views-label-field-pobox\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@POBox", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@POBox", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-phone\">    <span class=\"views-label views-label-field-office-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-fax\">    <span class=\"views-label views-label-field-office-fax\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficeFax", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficeFax", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-home-phone\">    <span class=\"views-label views-label-field-home-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@HomePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@HomePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-personal-mobile\">    <span class=\"views-label views-label-field-personal-mobile\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@PersonalMobile", subm.Groups[2].Value);

                                    }

                                }
                            else
                                command.Parameters.AddWithValue("@PersonalMobile", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-e-mail\">    <span class=\"views-label views-label-field-e-mail\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Email", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Email", "");


                            if (Mtype != 0)
                                command.Parameters.AddWithValue("@MembershipTypeid", Mtype);
                            if (SubUrl != null)
                                command.Parameters.AddWithValue("@Url", SubUrl);
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "select @@identity";
                        }
                    }
                }
                              
                        
                    

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ExtractPracticingMembers(int n, ref int j)
        {
            int Mtype = 2;

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    connection.Open();
                    for (int i = 0; i <= n; i++)
                    {
                        SqlCommand command = connection.CreateCommand();

                        string url = "http://grafium.solutions/dev/directory/practicing-members?page=" + Convert.ToString(i);

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string PageHtml = reader.ReadToEnd();
                        reader.Close();

                        Regex regex = new Regex("<div class=\"views-field views-field-field-number\">        <div class=\"field-content\">(.+?)</div>  </div>  </div>", RegexOptions.Multiline);
                        //Regex regexlp = new Regex("<a title=\"Go to last page\" class=\"btn btn-sm btn-default\" href=\"/dev/directory/trainees?page=(.+?)\">last", RegexOptions.Multiline);

                        //MatchCollection lpMatch = regexlp.Matches(PageHtml);
                        MatchCollection theMatches = regex.Matches(PageHtml);

                        foreach (Match m in theMatches)
                        {
                            Console.WriteLine("------");

                            string SubUrl = "http://www.grafium.solutions/dev/users/" + m.Groups[1].Value;

                            HttpWebRequest subrequest = (HttpWebRequest)WebRequest.Create(SubUrl);
                            HttpWebResponse subresponse = (HttpWebResponse)subrequest.GetResponse();

                            StreamReader subreader = new StreamReader(subresponse.GetResponseStream());
                            string SubPagehtml = subreader.ReadToEnd();
                            reader.Close();

                            command.CommandText = @"insert into Members (  Id
                                                                                  ,Name
                                                                                  ,MiddleName
                                                                                  ,Family
                                                                                  ,Title
                                                                                  ,FirmName
                                                                                  ,Mouhafaza
                                                                                  ,District
                                                                                  ,City
                                                                                  ,Street
                                                                                  ,Bldg
                                                                                  ,Floor
                                                                                  ,POBox
                                                                                  ,OfficePhone
                                                                                  ,OfficeFax
                                                                                  ,HomePhone
                                                                                  ,PersonalMobile
                                                                                  ,Email
                                                                                  ,MembershipTypeId
                                                                                  ,Url) values (   @Id
                                                                                                      ,@Name
                                                                                                      ,@Father
                                                                                                      ,@Family
                                                                                                      ,@Title
                                                                                                      ,@FirmName
                                                                                                      ,@Mouhafaza
                                                                                                      ,@District
                                                                                                      ,@City
                                                                                                      ,@Street
                                                                                                      ,@Bldg
                                                                                                      ,@Floor
                                                                                                      ,@POBox
                                                                                                      ,@OfficePhone
                                                                                                      ,@OfficeFax
                                                                                                      ,@HomePhone
                                                                                                      ,@PersonalMobile
                                                                                                      ,@Email
                                                                                                      ,@MembershipTypeId
                                                                                                      ,@Url)";
                            command.Parameters.Clear();

                            
                            command.Parameters.AddWithValue("@Id", j);
                            j++;
                            

                            Regex SubRegex = new Regex("<div class=\"views-field views-field-field-name\">    <span class=\"views-label views-label-field-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            MatchCollection theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Name", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Name", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-father\">    <span class=\"views-label views-label-field-father\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Father", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Father", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-family\">    <span class=\"views-label views-label-field-family\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Family", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Family", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-title\">    <span class=\"views-label views-label-field-title\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Title", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Title", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-firm-name\">    <span class=\"views-label views-label-field-firm-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@FirmName", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@FirmName", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-mouhafaza\">    <span class=\"views-label views-label-field-mouhafaza\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Mouhafaza", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Mouhafaza", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-district\">    <span class=\"views-label views-label-field-district\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@District", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@District", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-city\">    <span class=\"views-label views-label-field-city\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@City", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@City", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-street\">    <span class=\"views-label views-label-field-street\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Street", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Street", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-bldg\">    <span class=\"views-label views-label-field-bldg\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Bldg", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Bldg", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-floor\">    <span class=\"views-label views-label-field-floor\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Floor", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Floor", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-pobox\">    <span class=\"views-label views-label-field-pobox\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@POBox", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@POBox", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-phone\">    <span class=\"views-label views-label-field-office-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-fax\">    <span class=\"views-label views-label-field-office-fax\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficeFax", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficeFax", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-home-phone\">    <span class=\"views-label views-label-field-home-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@HomePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@HomePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-personal-mobile\">    <span class=\"views-label views-label-field-personal-mobile\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@PersonalMobile", subm.Groups[2].Value);

                                    }

                                }
                            else
                                command.Parameters.AddWithValue("@PersonalMobile", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-e-mail\">    <span class=\"views-label views-label-field-e-mail\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Email", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Email", "");


                            if (Mtype != 0)
                                command.Parameters.AddWithValue("@MembershipTypeid", Mtype);
                            if (SubUrl != null)
                                command.Parameters.AddWithValue("@Url", SubUrl);
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "select @@identity";
                        }
                    }


                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ExtractNonPracticingMembers(int n, ref int j)
        {
            int Mtype = 3;

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    connection.Open();
                    for (int i = 0; i <= n; i++)
                    {
                        SqlCommand command = connection.CreateCommand();

                        string url = "http://grafium.solutions/dev/directory/non-practicing-members?page=" + Convert.ToString(i);

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string PageHtml = reader.ReadToEnd();
                        reader.Close();

                        Regex regex = new Regex("<div class=\"views-field views-field-field-number\">        <div class=\"field-content\">(.+?)</div>  </div>  </div>", RegexOptions.Multiline);
                        //Regex regexlp = new Regex("<a title=\"Go to last page\" class=\"btn btn-sm btn-default\" href=\"/dev/directory/trainees?page=(.+?)\">last", RegexOptions.Multiline);

                        //MatchCollection lpMatch = regexlp.Matches(PageHtml);
                        MatchCollection theMatches = regex.Matches(PageHtml);

                        foreach (Match m in theMatches)
                        {
                            Console.WriteLine("------");

                            string SubUrl = "http://www.grafium.solutions/dev/users/" + m.Groups[1].Value;

                            HttpWebRequest subrequest = (HttpWebRequest)WebRequest.Create(SubUrl);
                            HttpWebResponse subresponse = (HttpWebResponse)subrequest.GetResponse();

                            StreamReader subreader = new StreamReader(subresponse.GetResponseStream());
                            string SubPagehtml = subreader.ReadToEnd();
                            reader.Close();

                            command.CommandText = @"insert into Members (  Id
                                                                                  ,Name
                                                                                  ,MiddleName
                                                                                  ,Family
                                                                                  ,Title
                                                                                  ,FirmName
                                                                                  ,Mouhafaza
                                                                                  ,District
                                                                                  ,City
                                                                                  ,Street
                                                                                  ,Bldg
                                                                                  ,Floor
                                                                                  ,POBox
                                                                                  ,OfficePhone
                                                                                  ,OfficeFax
                                                                                  ,HomePhone
                                                                                  ,PersonalMobile
                                                                                  ,Email
                                                                                  ,MembershipTypeId
                                                                                  ,Url) values (   @Id
                                                                                                      ,@Name
                                                                                                      ,@Father
                                                                                                      ,@Family
                                                                                                      ,@Title
                                                                                                      ,@FirmName
                                                                                                      ,@Mouhafaza
                                                                                                      ,@District
                                                                                                      ,@City
                                                                                                      ,@Street
                                                                                                      ,@Bldg
                                                                                                      ,@Floor
                                                                                                      ,@POBox
                                                                                                      ,@OfficePhone
                                                                                                      ,@OfficeFax
                                                                                                      ,@HomePhone
                                                                                                      ,@PersonalMobile
                                                                                                      ,@Email
                                                                                                      ,@MembershipTypeId
                                                                                                      ,@Url)";
                            command.Parameters.Clear();

                            
                            command.Parameters.AddWithValue("@Id", j);
                            j++;
                                        

                            Regex SubRegex = new Regex("<div class=\"views-field views-field-field-name\">    <span class=\"views-label views-label-field-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            MatchCollection theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Name", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Name", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-father\">    <span class=\"views-label views-label-field-father\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Father", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Father", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-family\">    <span class=\"views-label views-label-field-family\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Family", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Family", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-title\">    <span class=\"views-label views-label-field-title\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Title", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Title", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-firm-name\">    <span class=\"views-label views-label-field-firm-name\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@FirmName", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@FirmName", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-mouhafaza\">    <span class=\"views-label views-label-field-mouhafaza\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Mouhafaza", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Mouhafaza", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-district\">    <span class=\"views-label views-label-field-district\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@District", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@District", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-city\">    <span class=\"views-label views-label-field-city\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@City", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@City", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-street\">    <span class=\"views-label views-label-field-street\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Street", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Street", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-bldg\">    <span class=\"views-label views-label-field-bldg\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Bldg", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Bldg", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-floor\">    <span class=\"views-label views-label-field-floor\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Floor", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Floor", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-pobox\">    <span class=\"views-label views-label-field-pobox\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@POBox", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@POBox", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-phone\">    <span class=\"views-label views-label-field-office-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-office-fax\">    <span class=\"views-label views-label-field-office-fax\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@OfficeFax", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficeFax", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-home-phone\">    <span class=\"views-label views-label-field-home-phone\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@HomePhone", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@HomePhone", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-personal-mobile\">    <span class=\"views-label views-label-field-personal-mobile\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@PersonalMobile", subm.Groups[2].Value);

                                    }

                                }
                            else
                                command.Parameters.AddWithValue("@PersonalMobile", "");

                            SubRegex = new Regex("<div class=\"views-field views-field-field-e-mail\">    <span class=\"views-label views-label-field-e-mail\">(.+?)</span>    <div class=\"field-content\">(.+?)</div>  </div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 3)
                                    {
                                        if (subm.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Email", subm.Groups[2].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Email", "");


                            if (Mtype != 0)
                                command.Parameters.AddWithValue("@MembershipTypeid", Mtype);
                            if (SubUrl != null)
                                command.Parameters.AddWithValue("@Url", SubUrl);
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "select @@identity";
                        }
                    }


                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ExtractAccountancyFirms(int n, ref int j)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    connection.Open();
                    for (int i = 0; i <= n; i++)
                    {
                        SqlCommand command = connection.CreateCommand();

                        string url = "http://grafium.solutions/dev/directory/accountancy-firm?page=" + Convert.ToString(i);

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string PageHtml = reader.ReadToEnd();
                        reader.Close();

                        Regex regex = new Regex("<div class=\"views-field views-field-title\">        <span class=\"field-content\"><a href=\"/dev/directory/(.+?)\">(.+?)</a></span>  </div>", RegexOptions.Multiline);
                        //Regex regexlp = new Regex("<a title=\"Go to last page\" class=\"btn btn-sm btn-default\" href=\"/dev/directory/trainees?page=(.+?)\">last", RegexOptions.Multiline);

                        //MatchCollection lpMatch = regexlp.Matches(PageHtml);
                        MatchCollection theMatches = regex.Matches(PageHtml);

                        foreach (Match m in theMatches)
                        {
                            Console.WriteLine("------");

                            string SubUrl = "http://grafium.solutions/dev/directory/" + m.Groups[1].Value;

                            HttpWebRequest subrequest = (HttpWebRequest)WebRequest.Create(SubUrl);
                            HttpWebResponse subresponse = (HttpWebResponse)subrequest.GetResponse();

                            StreamReader subreader = new StreamReader(subresponse.GetResponseStream());
                            string SubPagehtml = subreader.ReadToEnd();
                            reader.Close();

                            command.CommandText = @"insert into AccountancyFirms (  Id
                                                                                      ,Url
                                                                                      ,Name
                                                                                      ,ManagingPartner
                                                                                      ,Mouhafaza
                                                                                      ,District
                                                                                      ,City
                                                                                      ,Street
                                                                                      ,Bldg
                                                                                      ,Floor
                                                                                      ,POBox
                                                                                      ,OfficePhone
                                                                                      ,OfficeFax
                                                                                      ,HomePhone
                                                                                      ,Email) values (   @Id
                                                                                      ,@Url
                                                                                      ,@Name
                                                                                      ,@ManagingPartner
                                                                                      ,@Mouhafaza
                                                                                      ,@District
                                                                                      ,@City
                                                                                      ,@Street
                                                                                      ,@Bldg
                                                                                      ,@Floor
                                                                                      ,@POBox
                                                                                      ,@OfficePhone
                                                                                      ,@OfficeFax
                                                                                      ,@HomePhone
                                                                                      ,@Email)";
                            command.Parameters.Clear();

                            Regex SubRegex = new Regex("<div class=\"field-label\">Number:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            MatchCollection theSubMatches = SubRegex.Matches(SubPagehtml);

                            
                            command.Parameters.AddWithValue("@Id", j);
                            j++;

                            command.Parameters.AddWithValue("@Url", SubUrl);
                            
                            if (m.Groups[2].Value != null)
                                            command.Parameters.AddWithValue("@Name", m.Groups[2].Value);
                            else command.Parameters.AddWithValue("@Name", "");

                            SubRegex = new Regex("<div class=\"field-label\">Managing Partner:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@ManagingPartner", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@ManagingPartner", "");


                            SubRegex = new Regex("<div class=\"field-label\">Mouhafaza:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@Mouhafaza", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Mouhafaza", "");

                    
                            SubRegex = new Regex("<div class=\"field-label\">District:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@District", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@District", "");

                            SubRegex = new Regex("<div class=\"field-label\">City:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@City", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@City", "");

                            SubRegex = new Regex("<div class=\"field-label\">Street:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@Street", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Street", "");

                            SubRegex = new Regex("<div class=\"field-label\">Bldg:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@Bldg", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Bldg", "");

                            SubRegex = new Regex("<div class=\"field-label\">Floor:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@Floor", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Floor", "");

                            SubRegex = new Regex("<div class=\"field-label\">POBox:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@POBox", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@POBox", "");

                            SubRegex = new Regex("<div class=\"field-label\">Office Phone:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@OfficePhone", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficePhone", "");

                            SubRegex = new Regex("<div class=\"field-label\">Office Fax:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@OfficeFax", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@OfficeFax", "");

                            SubRegex = new Regex("<div class=\"field-label\">Home Phone:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@HomePhone", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@HomePhone", "");

                            

                            SubRegex = new Regex("<div class=\"field-label\">E-mail:&nbsp;</div>(.+?)<div class=\"seperator\"></div>", RegexOptions.Multiline);

                            theSubMatches = SubRegex.Matches(SubPagehtml);

                            if (theSubMatches.Count > 0)
                                foreach (Match subm in theSubMatches)
                                {
                                    if (subm.Groups.Count == 2)
                                    {
                                        if (subm.Groups[1].Value != null)
                                            command.Parameters.AddWithValue("@Email", subm.Groups[1].Value);

                                    }

                                }
                            else command.Parameters.AddWithValue("@Email", "");


                            
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "select @@identity";
                        }
                    }


                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static string NumberofPages(string url)
        {
            try
            {
                
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string PageHtml = reader.ReadToEnd();
                    reader.Close();

                    Regex regex = new Regex("page=(.+?)\">last »", RegexOptions.Multiline);
                    
                    MatchCollection theMatches = regex.Matches(PageHtml);

                    foreach (Match m in theMatches)
                    {
                        return m.Groups[1].Value;
                    }
                    reader.Close();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "0";
            }

        return "0";
        }



    }

}