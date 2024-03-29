﻿#region Using statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.IO;
using System.Resources;
#endregion

namespace OGI_HR_Clanovi
{
    public partial class FormMembers : Form
    {
        private DataTable membersDataTable = new DataTable();
        private string strConnectionString = ConfigurationManager.ConnectionStrings["membersDatabase"].ConnectionString;
        private string imageFilePath = ConfigurationManager.AppSettings["imagePath"].ToString();

        public FormMembers()
        {
            InitializeComponent();
            FillDataGridView();
            tbcMembers.SelectTab("tbpMembersTable");
        }

        #region Buttons
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMembersForm())
            {   
                SaveImage();
                FillDataTable();
                MessageBox.Show("Uspješno ste spremili podatke!", "Obavijest");
                tbcMembers.SelectTab("tbpMembersTable");
                FillDataGridView();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Jeste li sigurni da želite obrisati podatke člana? (OPREZ! Podaci se ne povratno brišu!)",
                "Obavijest", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.Yes)
            {
                using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("MembersDelete", sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@MemberID", Convert.ToInt32(tbxMemberNumber.Text.Trim()));
                    sqlCommand.ExecuteNonQuery();
                    DeleteImage();
                }
                ClearForm();
                MessageBox.Show("Podaci uspješno obrisani!", "Obavijest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                tbcMembers.SelectTab("tbpMembersTable");
                FillDataGridView();
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }
        
        private void rbtnOrganizationYes_CheckedChanged(object sender, EventArgs e)
        {
            rtbxListOrganizations.Visible = true;
            lblListOrganizations.Visible = true;
        }

        private void rbtnOrganizationNo_CheckedChanged(object sender, EventArgs e)
        {
            rtbxListOrganizations.Visible = false;
            lblListOrganizations.Visible = false;
        }

        private void btnAddNewMember_Click(object sender, EventArgs e)
        {
            tbcMembers.SelectTab("tbpForm");
            ClearForm();
            EnableSaveButtons();
        }

        private void btnShowTable_Click(object sender, EventArgs e)
        {
            membersDataTable.Clear();
            tbcMembers.SelectTab("tbpMembersTable");
            FillDataGridView();
            EnableAllFormButtons();
            ClearForm();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            tbcMembers.SelectTab("tbpMembersTable");
            EnableAllFormButtons();
            
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (ValidateMembersForm())
            {
                UpdateImage();
                UpdateDataTable();
                MessageBox.Show("Podaci uspješno ažurirani!", "Obavijest");
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            tbcMembers.SelectTab("tbpMembersTable");
            rtbMailingList.Text = string.Empty;
            dgvMailingList.Columns.Clear();

        }

        private void btnAboutToExpire_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(strConnectionString))
            {
                connection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewAboutToExpire", connection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnPermInactive_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(strConnectionString))
            {
                connection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewLongInactive", connection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnInactive_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(strConnectionString))
            {
                connection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewInactive", connection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnExpired_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(strConnectionString))
            {
                connection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewRecentlyExpired", connection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(rtbMailingList.Text);
            }
            catch
            {
                MessageBox.Show("Nije moguće kopirati praznu listu!", "Obavijest", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM Members M WHERE M.Name LIKE '%' + @SearchString + '%' OR M.Surname LIKE '%' + @SearchString + '%' OR M.PersonalNumber LIKE '%' + @SearchString + '%' OR M.EMail LIKE '%' + @SearchString + '%'";
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(strConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchString", tbxSearch.Text);
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                    {
                        sqlDataAdapter.Fill(dataTable);
                    }
                }
            }
            btnCancelSearch.Visible = true;
            dgvMembers.DataSource = dataTable;
        }

        private void btnCancelSearch_Click(object sender, EventArgs e)
        {
            membersDataTable.Clear();
            FillDataGridView();
            tbxSearch.Text = String.Empty;
            btnCancelSearch.Visible = false;
        }

        private void btnAllEmails_Click(object sender, EventArgs e)
        {
            using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
            {
                sqlConnection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewAllEmails", sqlConnection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnActive_Click(object sender, EventArgs e)
        {
            using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
            {
                sqlConnection.Open();
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewActive", sqlConnection))
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    dgvMailingList.DataSource = dataTable;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rtbMailingList.AppendText(row[3].ToString() + ", ");
                    }
                }
            }
            tbcMembers.SelectTab("tbpMailingList");
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files |*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pbxMemberImage.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
            
        }

        private void btnExportImage_Click(object sender, EventArgs e)
        {
            if (ImageValidation())
            {
                string filePath = imageFilePath + "\\" + tbxName.Text + " " + tbxSurname.Text + ".jpg";
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JPEG (*.jpg)|*.jpg";
                    saveFileDialog.FileName = tbxMemberNumber.Text.ToString() + "_" + tbxName.Text.ToLower() + "_" + tbxSurname.Text.ToLower() + "_" + dtpDatePaid.Value.ToString("MM-yyyy");
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Image image = GetImageCopy(filePath);
                            image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            MessageBox.Show("Slika uspješno izvezena!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Dogodila se pogreška prilikom izvoza slike: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            tbcMembers.SelectTab("tbpMembersTable");
        }

        #endregion

        #region Functions
        private void ClearForm()
        {
            tbxName.Text = string.Empty;
            tbxSurname.Text = string.Empty;
            cbxGender.Text = string.Empty;
            tbxMemberNumber.Text = string.Empty;
            dtpDOB.Value = DateTime.Now;
            tbxPOB.Text = string.Empty;
            tbxNationality.Text = string.Empty;
            tbxPersonalNumber.Text = string.Empty;
            tbxDocumentID.Text = string.Empty;
            tbxAddressOfResidence.Text = string.Empty;
            tbxMailAddress.Text = string.Empty;
            tbxPhoneNumber.Text = string.Empty;
            tbxEMail.Text = string.Empty;
            tbxWebPage.Text = string.Empty;
            tbxProfession.Text = string.Empty;
            tbxMusicProfession.Text = string.Empty;
            rbtnPrimary.Checked = false;
            rbtnSecondary.Checked = false;
            tbxBasicSpecialty.Text = string.Empty;
            tbxAdditionalSpecialty.Text = string.Empty;
            tbxBandName.Text = string.Empty;
            tbxStageName.Text = string.Empty;
            tbxManager.Text = string.Empty;
            tbxPublisher.Text = string.Empty;
            rbtnOrganizationYes.Checked = false;
            rbtnOrganizationNo.Checked = false;
            rtbxListOrganizations.Text = string.Empty;
            rbtnEquipmentYes.Checked = false;
            rbtnEquipmentNo.Checked = false;
            tbxMusicCategory.Text = string.Empty;
            rtbxBiography.Text = string.Empty;
            rbtnActive.Checked = false;
            rbtnHonorary.Checked = false;
            rbtnJoined.Checked = false;
            dtpDatePaid.Value = DateTime.Now;
            pbxMemberImage.Image = Properties.Resources.placeholder;
        }

        private bool ValidateMembersForm()
        {
            bool _isValid = true;
            if (tbxMemberNumber.Text == string.Empty)
            {
                MessageBox.Show("Potrebno je postaviti broj člana!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isValid = false;
            }
            if (tbxPersonalNumber.Text == string.Empty)
            {
                MessageBox.Show("Potrebno je postaviti OIB!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isValid = false;
            }
            if (rbtnActive.Checked == false && rbtnHonorary.Checked == false && rbtnJoined.Checked == false)
            {
                MessageBox.Show("Potrebno je postaviti vrstu članstva!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isValid = false;
            }

            Image currentImage = pbxMemberImage.Image;
            Image resourceImage = Properties.Resources.placeholder;
            byte[] currentImageData = ImageToByteArray(currentImage);
            byte[] resourceImageData = ImageToByteArray(resourceImage);
            if (ByteArraysEqual(currentImageData, resourceImageData))
            {
                MessageBox.Show("Potrebno je postaviti sliku člana!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isValid = false;
            }

            return _isValid;
        }

        private bool ImageValidation()
        {
            Image currentImage = pbxMemberImage.Image;
            Image resourceImage = Properties.Resources.placeholder;
            byte[] currentImageData = ImageToByteArray(currentImage);
            byte[] resourceImageData = ImageToByteArray(resourceImage);
            currentImage.Dispose();
            resourceImage.Dispose();
            if (ByteArraysEqual(currentImageData, resourceImageData))
            {
                MessageBox.Show("Potrebno je postaviti sliku člana!", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else return true;
            
        }

        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try { image.Save(ms, ImageFormat.Jpeg); }
                catch (Exception ex)
                {
                    MessageBox.Show($"Došlo je do pogreške{ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return ms.ToArray(); 
            }
        }

        private void SaveImage()
        {
            string filePath = imageFilePath + "\\" + tbxName.Text + " " + tbxSurname.Text + ".jpg";
            try
            {
                pbxMemberImage.Image.Save(filePath, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dogodila se pogreška prilikom spremanja slike: {ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateImage()
        {
            DeleteImage();
            SaveImage();
        }

        private void RetrieveImage()
        {
            string filePath = imageFilePath + "\\" + tbxName.Text + " " + tbxSurname.Text + ".jpg";
            try
            {
                if (File.Exists(filePath))
                {
                    pbxMemberImage.Image.Dispose();
                    pbxMemberImage.Image = GetImageCopy(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dogodila se pogreška prilikom dohvata slike: {ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteImage()
        {
            string filePath = imageFilePath + "\\" + tbxName.Text + " " + tbxSurname.Text + ".jpg";
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dogodila se pogreška prilikom brisanja slike: {ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private Image GetImageCopy(string imagePath)
        {
            using(Image image = Image.FromFile(imagePath))
            {
                Bitmap bitmap = new Bitmap(image);
                return bitmap;
            }
        }

        private bool ByteArraysEqual(byte[] array1, byte[] array2)
        {
            if(array1.Length != array2.Length){
                return false;
            }

            for(int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }

        private void FillDataTable()
        {
            using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("MembersAdd", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@MemberID", Convert.ToInt32(tbxMemberNumber.Text.Trim()));
                sqlCommand.Parameters.AddWithValue("@Name", tbxName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Surname", tbxSurname.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Gender", cbxGender.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                sqlCommand.Parameters.AddWithValue("@POB", tbxPOB.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Nationality", tbxNationality.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@PersonalNumber", tbxPersonalNumber.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@DocumentIDNumber", tbxDocumentID.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@AddressOfResidence", tbxAddressOfResidence.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MailAddress", tbxMailAddress.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", tbxPhoneNumber.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@EMail", tbxEMail.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@WebPage", tbxWebPage.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Profession", tbxProfession.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MusicProfession", tbxMusicProfession.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MusicActivityType", rbtnPrimary.Checked ? "Primarna djelatnost" : "Dopunska djelatnost");
                sqlCommand.Parameters.AddWithValue("@BasicSpecialty", tbxBasicSpecialty.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@AdditionalSpecialty", tbxAdditionalSpecialty.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@BandName", tbxBandName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@StageName", tbxStageName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@ManagerName", tbxManager.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Publisher", tbxPublisher.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MemberOfOtherOrganizations", rbtnOrganizationYes.Checked ? "Da" : "Ne");
                if (rbtnOrganizationYes.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@OtherOrganizations", rtbxListOrganizations.Text.Trim());
                }
                else sqlCommand.Parameters.AddWithValue("@OtherOrganizations", string.Empty);
                sqlCommand.Parameters.AddWithValue("@OwnEquipment", rbtnEquipmentYes.Checked ? "Da" : "Ne");
                sqlCommand.Parameters.AddWithValue("@MusicCategory", tbxMusicCategory.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Biography", rtbxBiography.Text.Trim());
                if (rbtnActive.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Aktivno");
                }
                if (rbtnHonorary.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Pocasno");
                }
                if (rbtnJoined.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Pridruzeno");
                }
                sqlCommand.Parameters.AddWithValue("@DatePaid", dtpDatePaid.Value);
                sqlCommand.ExecuteScalar();
            }
        }

        private void FillDataGridView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewAll", sqlConnection);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                dgvMembers.DataSource = dataTable;
            }
        }

        private void FillFormDataFromTable(DataRow dataRow)
        {
            tbxName.Text = dataRow["Name"].ToString();
            tbxSurname.Text = dataRow["Surname"].ToString();
            cbxGender.Text = dataRow["Gender"].ToString();
            tbxMemberNumber.Text = dataRow["MemberID"].ToString();
            dtpDOB.Value = Convert.ToDateTime(dataRow["DOB"]);
            tbxPOB.Text = dataRow["POB"].ToString();
            tbxNationality.Text = dataRow["Nationality"].ToString();
            tbxPersonalNumber.Text = dataRow["PersonalNumber"].ToString();
            tbxDocumentID.Text = dataRow["DocumentIDNumber"].ToString();
            tbxAddressOfResidence.Text = dataRow["AddressOfResidence"].ToString();
            tbxMailAddress.Text = dataRow["MailAddress"].ToString();
            tbxPhoneNumber.Text = dataRow["PhoneNumber"].ToString();
            tbxEMail.Text = dataRow["EMail"].ToString();
            tbxWebPage.Text = dataRow["WebPage"].ToString();
            tbxProfession.Text = dataRow["Profession"].ToString();
            tbxMusicProfession.Text = dataRow["MusicProfession"].ToString();
            if (dataRow["MusicActivityType"].ToString() == "Primarna djelatnost")
            {
                rbtnPrimary.Checked = true;
            }
            else rbtnSecondary.Checked = true;
            tbxBasicSpecialty.Text = dataRow["BasicSpecialty"].ToString();
            tbxAdditionalSpecialty.Text = dataRow["AdditionalSpecialty"].ToString();
            tbxBandName.Text = dataRow["BandName"].ToString();
            tbxStageName.Text = dataRow["StageName"].ToString();
            tbxManager.Text = dataRow["ManagerName"].ToString();
            tbxPublisher.Text = dataRow["Publisher"].ToString();
            if (dataRow["MemberOfOtherOrganizations"].ToString() == "Da")
            {
                rbtnOrganizationYes.Checked = true;
            }
            else rbtnOrganizationNo.Checked = true;
            rtbxListOrganizations.Text = dataRow["OtherOrganizations"].ToString();
            if (dataRow["OwnEquipment"].ToString() == "Da")
            {
                rbtnEquipmentYes.Checked = true;
            }
            else rbtnEquipmentNo.Checked = true;
            tbxMusicCategory.Text = dataRow["MusicCategory"].ToString();
            rtbxBiography.Text = dataRow["Biography"].ToString();
            if (dataRow["MembershipType"].ToString() == "Aktivno")
            {
                rbtnActive.Checked = true;
            }
            else if (dataRow["MembershipType"].ToString() == "Pocasno")
            {
                rbtnHonorary.Checked = true;
            }
            else rbtnJoined.Checked = true;
            dtpDatePaid.Value = Convert.ToDateTime(dataRow["DatePaid"]);
            RetrieveImage();
        }

        private void EnableAllFormButtons()
        {
            btnUpdate.Enabled = true;
            btnUpdate.Visible = true;
            btnDelete.Enabled = true;
            btnDelete.Visible = true;
            btnSave.Enabled = true;
            btnSave.Visible = true;
            btnCancel.Enabled = true;
            btnCancel.Visible = true;
            
        }

        private void EnableSaveButtons()
        {
            btnUpdate.Enabled = false;
            btnUpdate.Visible = false;
            btnDelete.Enabled = false;
            btnDelete.Visible = false;
            btnSave.Enabled = true;
            btnSave.Visible = true;
            btnCancel.Enabled = true;
            btnCancel.Visible = true;
            
        }

        private void EnableUpdateButtons()
        {
            btnUpdate.Enabled = true;
            btnUpdate.Visible = true;
            btnDelete.Enabled = true;
            btnDelete.Visible = true;
            btnSave.Enabled = false;
            btnSave.Visible = false;
            btnCancel.Enabled = false;
            btnCancel.Visible = false;

        }

        private void UpdateDataTable()
        {
            using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("MembersEdit", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@MemberID", Convert.ToInt32(tbxMemberNumber.Text.Trim()));
                sqlCommand.Parameters.AddWithValue("@Name", tbxName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Surname", tbxSurname.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Gender", cbxGender.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                sqlCommand.Parameters.AddWithValue("@POB", tbxPOB.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Nationality", tbxNationality.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@PersonalNumber", tbxPersonalNumber.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@DocumentIDNumber", tbxDocumentID.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@AddressOfResidence", tbxAddressOfResidence.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MailAddress", tbxMailAddress.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@PhoneNumber", tbxPhoneNumber.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@EMail", tbxEMail.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@WebPage", tbxWebPage.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Profession", tbxProfession.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MusicProfession", tbxMusicProfession.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MusicActivityType", rbtnPrimary.Checked ? "Primarna djelatnost" : "Dopunska djelatnost");
                sqlCommand.Parameters.AddWithValue("@BasicSpecialty", tbxBasicSpecialty.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@AdditionalSpecialty", tbxAdditionalSpecialty.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@BandName", tbxBandName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@StageName", tbxStageName.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@ManagerName", tbxManager.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Publisher", tbxPublisher.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@MemberOfOtherOrganizations", rbtnOrganizationYes.Checked ? "Da" : "Ne");
                if (rbtnOrganizationYes.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@OtherOrganizations", rtbxListOrganizations.Text.Trim());
                }
                else sqlCommand.Parameters.AddWithValue("@OtherOrganizations", string.Empty);
                sqlCommand.Parameters.AddWithValue("@OwnEquipment", rbtnEquipmentYes.Checked ? "Da" : "Ne");
                sqlCommand.Parameters.AddWithValue("@MusicCategory", tbxMusicCategory.Text.Trim());
                sqlCommand.Parameters.AddWithValue("@Biography", rtbxBiography.Text.Trim());
                if (rbtnActive.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Aktivno");
                }
                if (rbtnHonorary.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Pocasno");
                }
                if (rbtnJoined.Checked)
                {
                    sqlCommand.Parameters.AddWithValue("@MembershipType", "Pridruzeno");
                }
                sqlCommand.Parameters.AddWithValue("@DatePaid", dtpDatePaid.Value);

                sqlCommand.ExecuteScalar();
            }
        }


        #endregion

        #region TabControl
        private void tbcMembers_MouseClick(object sender, MouseEventArgs e)
        {
            FillDataGridView();
        }


        #endregion

        #region DataGridView
        private void dgvMembers_DoubleClick(object sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow.Index != -1) //da se ne moze kliknit header red
            {
                DataGridViewRow mCurrentRow = dgvMembers.CurrentRow;
                using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewById", sqlConnection);
                    sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@MemberID", Convert.ToInt32(mCurrentRow.Cells[0].Value));
                    DataSet mDataSet = new DataSet();
                    sqlDataAdapter.Fill(mDataSet);
                    DataRow mDataRow = mDataSet.Tables[0].Rows[0];
                    FillFormDataFromTable(mDataRow);
                    tbcMembers.SelectTab("tbpForm");
                    EnableUpdateButtons();
                }

            }
        }

        private void dgvMailingList_DoubleClick(object sender, EventArgs e)
        {
            rtbMailingList.Text = String.Empty;
            if (dgvMembers.CurrentRow.Index != -1) //da se ne moze kliknit header red
            {
                DataGridViewRow mCurrentRow = dgvMailingList.CurrentRow;
                using (SqlConnection sqlConnection = new SqlConnection(strConnectionString))
                {
                    sqlConnection.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("MembersViewById", sqlConnection);
                    sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@MemberID", Convert.ToInt32(mCurrentRow.Cells[4].Value));
                    DataSet mDataSet = new DataSet();
                    sqlDataAdapter.Fill(mDataSet);
                    DataRow mDataRow = mDataSet.Tables[0].Rows[0];
                    FillFormDataFromTable(mDataRow);
                    tbcMembers.SelectTab("tbpForm");
                    EnableUpdateButtons();
                }

            }
        }

        #endregion

        #region Textbox
        private void tbxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                btnSearch_Click(sender, e);
            }
        }






        #endregion

    }
}
