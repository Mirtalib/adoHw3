using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Task3_Ado_async;

public partial class NewAsync : Form
{
    IConfigurationRoot? configuration = null;
    SqlConnection? conn = null;
    SqlDataReader? reader = null;
    string? conStr = null;

    public NewAsync()
    {
        InitializeComponent();
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        conStr = configuration.GetConnectionString("ConStr");
        addDataDb();
    }



    private async void addDataDb()
    {
        conn = new SqlConnection(conStr);
        if (conn is null)
            return;
        try
        {
            await conn.OpenAsync();
            string selCom = "SELECT NAME FROM Categories";
            SqlCommand commmand = conn.CreateCommand();
            commmand.CommandText = selCom;
            reader = await commmand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {

                CategoryCmbx.Items.Add(reader[0]);
            }

        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
        finally
        {
            conn?.CloseAsync();
            reader?.CloseAsync();
        }
    }

    private async void CategoryCmbx_SelectedIndexChanged(object sender, EventArgs e)
    {

        if (conn is null || reader is null)
            return;

        try
        {
            await conn.OpenAsync();
            string selectedString = @"
                                      Select Authors.FirstName
                                      From Books
                                      JOIN Authors ON Authors.Id=Id_Author
                                      JOIN Categories ON Id_Category=Categories.Id
                                      WHERE Categories.Name=@p1";
            SqlCommand command = conn.CreateCommand();
            command.CommandText = selectedString;
            command.Parameters.AddWithValue("@p1", CategoryCmbx.SelectedItem.ToString());
            reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                AuthorsCmbx.Items.Add(reader[0]);
            }
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
        finally
        {
            conn?.CloseAsync();
            reader?.CloseAsync();
        }
    }

    private async void ExecuteBtn_Click(object sender, EventArgs e)
    {

        if (conn is null || reader is null)
            return;


        try
        {
            await conn.OpenAsync();

            string selectedString = @"SELECT Books.Name,Books.Pages,Books.YearPress,Books.Comment,Books.Quantity
                                       FROM Books
                                       JOIN Authors ON Authors.Id=Id_Author
                                       JOIN Categories ON Categories.Id=Books.Id_Category
                                       WHERE Authors.FirstName=@p1 AND Categories.[Name] = @p2";

            SqlCommand command = conn.CreateCommand();
            command.Parameters.AddWithValue("@p1", AuthorsCmbx.SelectedItem.ToString());
            command.Parameters.AddWithValue("@p2", CategoryCmbx.SelectedItem.ToString());
            command.CommandText = selectedString;
            DataTable dataTable = new DataTable();

            reader = await command.ExecuteReaderAsync();

     

            int line = 0;

            do
            {
                while (await reader.ReadAsync())
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
                        row[i] = await reader.GetFieldValueAsync<object>(i);


                    dataTable.Rows.Add(row);
                }
            } while (reader.NextResult());

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = dataTable;
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
        finally
        {
            await conn.CloseAsync();
            await reader.CloseAsync();
        }
    }
}