#region Using statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.IO;
#endregion

namespace OGI_HR_Clanovi
{
    public partial class FormMembers : Form
    {
        private DataTable membersDataTable = new DataTable();
        private string strConnectionString = ConfigurationManager.ConnectionStrings["membersDatabase"].ConnectionString;

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
                    sqlCommand.ExecuteScalar();
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
        }

        private bool ValidateMembersForm()
        {
            bool _isValid = true;
            if (tbxMemberNumber.Text == string.Empty)
            {
                MessageBox.Show("Member number is required");
                _isValid = false;
            }
            if (tbxPersonalNumber.Text == string.Empty)
            {
                MessageBox.Show("Personal number is required");
                _isValid = false;
            }
            if (rbtnActive.Checked == false && rbtnHonorary.Checked == false && rbtnJoined.Checked == false)
            {
                MessageBox.Show("Type of membership is required");
                _isValid = false;
            }

            return _isValid;
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

        private void FillFormDataFromTable(DataRow mDataRow)
        {
            tbxName.Text = mDataRow["Name"].ToString();
            tbxSurname.Text = mDataRow["Surname"].ToString();
            cbxGender.Text = mDataRow["Gender"].ToString();
            tbxMemberNumber.Text = mDataRow["MemberID"].ToString();
            dtpDOB.Value = Convert.ToDateTime(mDataRow["DOB"]);
            tbxPOB.Text = mDataRow["POB"].ToString();
            tbxNationality.Text = mDataRow["Nationality"].ToString();
            tbxPersonalNumber.Text = mDataRow["PersonalNumber"].ToString();
            tbxDocumentID.Text = mDataRow["DocumentIDNumber"].ToString();
            tbxAddressOfResidence.Text = mDataRow["AddressOfResidence"].ToString();
            tbxMailAddress.Text = mDataRow["MailAddress"].ToString();
            tbxPhoneNumber.Text = mDataRow["PhoneNumber"].ToString();
            tbxEMail.Text = mDataRow["EMail"].ToString();
            tbxWebPage.Text = mDataRow["WebPage"].ToString();
            tbxProfession.Text = mDataRow["Profession"].ToString();
            tbxMusicProfession.Text = mDataRow["MusicProfession"].ToString();
            if (mDataRow["MusicActivityType"].ToString() == "Primarna djelatnost")
            {
                rbtnPrimary.Checked = true;
            }
            else rbtnSecondary.Checked = true;
            tbxBasicSpecialty.Text = mDataRow["BasicSpecialty"].ToString();
            tbxAdditionalSpecialty.Text = mDataRow["AdditionalSpecialty"].ToString();
            tbxBandName.Text = mDataRow["BandName"].ToString();
            tbxStageName.Text = mDataRow["StageName"].ToString();
            tbxManager.Text = mDataRow["ManagerName"].ToString();
            tbxPublisher.Text = mDataRow["Publisher"].ToString();
            if (mDataRow["MemberOfOtherOrganizations"].ToString() == "Da")
            {
                rbtnOrganizationYes.Checked = true;
            }
            else rbtnOrganizationNo.Checked = true;
            rtbxListOrganizations.Text = mDataRow["OtherOrganizations"].ToString();
            if (mDataRow["OwnEquipment"].ToString() == "Da")
            {
                rbtnEquipmentYes.Checked = true;
            }
            else rbtnEquipmentNo.Checked = true;
            tbxMusicCategory.Text = mDataRow["MusicCategory"].ToString();
            rtbxBiography.Text = mDataRow["Biography"].ToString();
            if (mDataRow["MembershipType"].ToString() == "Aktivno")
            {
                rbtnActive.Checked = true;
            }
            else if (mDataRow["MembershipType"].ToString() == "Pocasno")
            {
                rbtnHonorary.Checked = true;
            }
            else rbtnJoined.Checked = true;
            dtpDatePaid.Value = Convert.ToDateTime(mDataRow["DatePaid"]);


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
