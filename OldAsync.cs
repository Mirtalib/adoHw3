using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Task3_Ado_async;

public partial class OldAsync : Form
{
    DataTable? dataTable = null;
    IConfigurationRoot? configuration = null;
    SqlConnection? conn = null;
    SqlDataReader? reader = null;
    string conStr = string.Empty;
    public OldAsync()
    {
        InitializeComponent();
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        if (configuration.GetConnectionString("ConStr") != null)
            conStr = configuration.GetConnectionString("ConStr")!;
        addDataDb();
    }



    private void addDataDb()
    {
        conn = new SqlConnection(conStr);
        SqlCommand command = conn.CreateCommand();
        string selcommand = "SELECT NAME FROM Categories";
        command.CommandText =   selcommand;

        try
        {
            conn?.Open();

            IAsyncResult iar = command.BeginExecuteReader();
            WaitHandle handle = iar.AsyncWaitHandle;
            if (handle.WaitOne(10000))
            {
                reader = command.EndExecuteReader(iar);
                do
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            CategoryCmbx.Items.Add(reader[i]);
                        }
                    }
                } while (reader.NextResult());
            }
            else
                MessageBox.Show("TimeOut exceeded");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            if (reader is not null && !reader.IsClosed)
                reader.Close();
            conn.Close();
        }
    }



    private void CategoryCmbx_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (conn is null)
            return;
        

        SqlCommand command = conn.CreateCommand();
        string selcommand = @"
                                      Select Authors.FirstName
                                      From Books
                                      JOIN Authors ON Authors.Id=Id_Author
                                      JOIN Categories ON Id_Category=Categories.Id
                                      WHERE Categories.Name=@p1";
        command.Parameters.AddWithValue("@p1", CategoryCmbx.SelectedItem.ToString());

        var commandtxt = "WAITFOR DELAY '00:00:02'";
        command.CommandText = commandtxt + selcommand;


        try
        {
            conn?.Open();

            IAsyncResult iar = command.BeginExecuteReader();
            WaitHandle handle = iar.AsyncWaitHandle;
            if (handle.WaitOne(10000))
            {
                reader = command.EndExecuteReader(iar);

                do
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            AuthorsCmbx.Items.Add(reader[i]);
                        }

                    }
                } while (reader.NextResult());
            }
            else
            {
                MessageBox.Show("TimeOut exceeded");
            }
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
        finally
        {
            if (reader is not null && !reader.IsClosed)
                reader.Close();
            conn.Close();
        }
    }
    private void ExecuteBtn_Click(object sender, EventArgs e)
    {
        if (conn is null)
            return;


        SqlCommand command = conn.CreateCommand();
        var selcommand = @"SELECT Books.Name,Books.Pages,Books.YearPress,Books.Comment,Books.Quantity
                                       FROM Books
                                       JOIN Authors ON Authors.Id=Id_Author
                                       JOIN Categories ON Categories.Id=Books.Id_Category
                                       WHERE Authors.FirstName=@p1 AND Categories.[Name] = @p2";

        var commandtxt = "WAITFOR DELAY '00:00:02'";

        command.CommandText = commandtxt + selcommand;

        command.Parameters.AddWithValue("@p1", AuthorsCmbx.SelectedItem.ToString());
        command.Parameters.AddWithValue("@p2", CategoryCmbx.SelectedItem.ToString());

        try
        {
            conn?.Open();
            IAsyncResult iar = command.BeginExecuteReader();
            WaitHandle handle = iar.AsyncWaitHandle;
            if (handle.WaitOne(10000))
            {
                getData(command, iar);
            }
            else
            {
                MessageBox.Show("TimeOut exceeded");
            }
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
    }

    private void getData(SqlCommand command, IAsyncResult ia)
    {
        try
        {
            reader = command.EndExecuteReader(ia);
            dataTable = new DataTable();

            int line = 0;

            do
            {
                while (reader.Read())
                {
                    if (line == 0)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataTable.Columns.Add(reader.GetName(i));
                        }
                        line++;
                    }

                    DataRow row = dataTable.NewRow();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader[i];
                    }

                    dataTable.Rows.Add(row);
                }
            } while (reader.NextResult());


            dataGridView1.DataSource = dataTable;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            if (reader is not null && !reader.IsClosed)
                reader.Close();
            conn?.Close();
        }
    }

}
