using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Numerics;
using System.Windows.Forms;

namespace Ortalama_Vade_Hesaplama
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            labelSonuc.Text = DateTime.Now.ToString("dd:MM:yyyy");
        }
        private void dosya_yukle_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                LoadDataIntoDataGridView(filePath);
            }
        }

        private void LoadDataIntoDataGridView(string filePath)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                workbook = excelApp.Workbooks.Open(filePath);
                worksheet = workbook.Sheets[1];

                DataTable dt = new DataTable();
                int columnCount = worksheet.UsedRange.Columns.Count;

                // Kolon isimleri
                for (int col = 1; col <= columnCount; col++)
                {
                    dt.Columns.Add("Kolon" + col);
                }

                // Verileri oku
                for (int row = 1; row <= worksheet.UsedRange.Rows.Count; row++)
                {
                    DataRow dataRow = dt.NewRow();
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var cellValue = (worksheet.Cells[row, col] as Excel.Range).Value2;

                        // Veri tür kontrolü 
                        if (cellValue is double)
                        {
                            dataRow[col - 1] = (double)cellValue; // Sayı
                        }
                        else if (cellValue is DateTime)
                        {
                            dataRow[col - 1] = ((DateTime)cellValue).ToString("yyyy-MM-dd"); // Tarih
                        }
                        else
                        {
                            dataRow[col - 1] = cellValue?.ToString() ?? string.Empty;  //Boş
                        }
                    }
                    dt.Rows.Add(dataRow);
                }

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (worksheet != null) Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
            }
        }

        private void satir_carp_Click(object sender, EventArgs e)
        {
            // Sonuç kolonu kontrolü 
            if (!dataGridView1.Columns.Contains("Sonuç"))
            {
                dataGridView1.Columns.Add("Sonuç", "Sonuç");
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Kolon Satır Kontrolü
                if (row.Cells["Kolon1"].Value != null && row.Cells["Kolon2"].Value != null)
                {
                    if (BigInteger.TryParse(row.Cells["Kolon1"].Value.ToString(), out BigInteger number1) &&
                        BigInteger.TryParse(row.Cells["Kolon2"].Value.ToString(), out BigInteger number2))
                    {
                        // Çarpım sonucunu "Sonuç" hücresine yazdır
                        BigInteger resultNumber = number1 * number2;
                        row.Cells["Sonuç"].Value = resultNumber;
                    }
                }
            }
        }
        private void KolonTopla()
        {
            // İlk sütundaki değerleri toplama
            BigInteger sum = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Sonuç"].Value != null &&
                    BigInteger.TryParse(row.Cells["Sonuç"].Value.ToString(), out BigInteger cellValue))
                {
                    sum += cellValue;
                }
            }

            // İkinci sütundaki değerleri olarak toplama
            BigInteger totalPayment = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Kolon2"].Value != null &&
                    BigInteger.TryParse(row.Cells["Kolon2"].Value.ToString(), out BigInteger cellValue))
                {
                    totalPayment += cellValue;
                }
            }

            // Sıfıra bölme hatasını önleme
            if (totalPayment == 0)
            {
                MessageBox.Show("Toplam ödeme 0 olamaz. Bölme işlemi yapılamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; 
            }

            // Sonucu hesaplama
            BigInteger callResult = sum / totalPayment;

            // Gün eklemeye başlamak için başlangıç tarihini ayarlayın
            DateTime baseDate = new DateTime(1900, 1, 1);

            // Gün olarak kullanmak için int'e dönüştürme
            int totalDays;
            if (callResult <= int.MaxValue)
            {
                totalDays = (int)callResult;
            }
            else
            {
                MessageBox.Show("Sonuç çok büyük, tarih hesaplanamıyor.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txt_vade_farki.Text != string.Empty)
            {
                DateTime resultDate = baseDate.AddDays(totalDays + Convert.ToInt32(txt_vade_farki.Text) - 2);

                // Sonucu label göster
                labelSonuc.Text = $"Sonuç Tarihi: \n {resultDate.ToShortDateString()}";

                MessageBox.Show("Sonuç Tarihi:\n " + resultDate.ToShortDateString(), "Hesaplanan Tarih \n", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Başlangıç tarihine gün ekleyerek yeni tarihi hesapla
                DateTime resultDate = baseDate.AddDays(totalDays - 2);

                // Sonucu bir label üzerinde göster
                labelSonuc.Text = $"Sonuç Tarihi: \n {resultDate.ToShortDateString()}";

                MessageBox.Show("Sonuç Tarihi:\n " + resultDate.ToShortDateString(), "Hesaplanan Tarih \n", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }
        private void sonuc_getir_Click(object sender, EventArgs e)
        {
            KolonTopla();
        }
    }
}
