using De02.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace De02
{
    public partial class frmSanPham : Form
    {
        public frmSanPham()
        {
            InitializeComponent();
        }

        private void frmSanPham_Load(object sender, EventArgs e)
        {
            try
            {
                QLSanPhamContextDB db = new QLSanPhamContextDB();
                List<LoaiSP> listLoaiSP = db.LoaiSPs.ToList();
                List<Sanpham> listSanPham = db.Sanphams.ToList();
                FillLoaiSPComboBox(listLoaiSP);
                BindGrid(listSanPham);
                btnLuu.Enabled = false;
                btnKLuu.Enabled = false;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillLoaiSPComboBox(List<LoaiSP> listLoaiSP)
        {
            cboLoaiSP.DataSource = listLoaiSP;
            cboLoaiSP.DisplayMember = "TenLoai";
            cboLoaiSP.ValueMember = "MaLoai";
            cboLoaiSP.SelectedIndexChanged += (s, e) => CheckAndEnableSaveButtons();
        }

        private void BindGrid(List<Sanpham> listSanPham)
        {
            dgvSanpham.Rows.Clear();
            foreach (var item in listSanPham)
            {
                int index = dgvSanpham.Rows.Add();
                dgvSanpham.Rows[index].Cells[0].Value = item.MaSP;
                dgvSanpham.Rows[index].Cells[1].Value = item.TenSP;
                dgvSanpham.Rows[index].Cells[2].Value = item.Ngaynhap.ToString("dd/MM/yyyy HH:mm:ss");
                dgvSanpham.Rows[index].Cells[3].Value = item.LoaiSP.TenLoai;
            }
        }

        private void dgvSanpham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSanpham.Rows[e.RowIndex];
                    txtMaSP.Text = row.Cells[0].Value?.ToString();
                    txtTenSP.Text = row.Cells[1].Value?.ToString();
                    string dateValue = row.Cells[2].Value?.ToString();
                    DateTime ngayNhap;
                    if (DateTime.TryParse(dateValue, out ngayNhap))
                    {
                        dtNgayNhap.Value = ngayNhap; 
                    }
                    else
                    {
                        MessageBox.Show("Ngày không hợp lệ: " + dateValue);
                    }
                    cboLoaiSP.Text = row.Cells[3].Value?.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn dòng: " + ex.Message);
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                QLSanPhamContextDB db = new QLSanPhamContextDB();
                if (db.Sanphams.Any(sp => sp.MaSP == txtMaSP.Text))
                {
                    MessageBox.Show("Mã sản phẩm đã tồn tại. Vui lòng nhập mã khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTenSP.Text) || cboLoaiSP.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var newSanPham = new Sanpham
                {
                    MaSP = txtMaSP.Text,
                    TenSP = txtTenSP.Text,
                    Ngaynhap = dtNgayNhap.Value,
                    MaLoai = cboLoaiSP.SelectedValue.ToString()
                };
                db.Sanphams.Add(newSanPham);
                db.SaveChanges();

                BindGrid(db.Sanphams.ToList());
                MessageBox.Show("Thêm sản phẩm mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                QLSanPhamContextDB db = new QLSanPhamContextDB();

                if (string.IsNullOrWhiteSpace(txtMaSP.Text))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var existingProduct = db.Sanphams.FirstOrDefault(x => x.MaSP == txtMaSP.Text);
                if (existingProduct == null)
                {
                    MessageBox.Show("Không tìm thấy sản phẩm cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var duplicateProduct = db.Sanphams.FirstOrDefault(x => x.MaSP != txtMaSP.Text && x.TenSP == txtTenSP.Text);
                if (duplicateProduct != null)
                {
                    MessageBox.Show("Sản phẩm này đã tồn tại trong hệ thống. Vui lòng nhập tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                existingProduct.TenSP = txtTenSP.Text;
                existingProduct.Ngaynhap = dtNgayNhap.Value;
                existingProduct.MaLoai = cboLoaiSP.SelectedValue.ToString();

                db.SaveChanges();

                BindGrid(db.Sanphams.ToList());

                MessageBox.Show("Sửa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa sản phẩm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMaSP.Text))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult dialogResult = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    using (QLSanPhamContextDB db = new QLSanPhamContextDB())
                    {
                        var sp = db.Sanphams.FirstOrDefault(x => x.MaSP == txtMaSP.Text);

                        if (sp == null)
                        {
                            MessageBox.Show("Sản phẩm không tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        db.Sanphams.Remove(sp);
                        db.SaveChanges();  
                        BindGrid(db.Sanphams.ToList());
                        MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            try
            {
                QLSanPhamContextDB db = new QLSanPhamContextDB();
                string keyword = txtTim.Text.ToLower();

                var result = db.Sanphams
                    .Where(x => x.MaSP.ToLower().Contains(keyword) ||
                                x.TenSP.ToLower().Contains(keyword) ||
                                x.LoaiSP.TenLoai.ToLower().Contains(keyword))
                    .ToList();

                if (result.Count > 0)
                {
                    BindGrid(result);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sản phẩm nào phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
           "Bạn có chắc chắn muốn thoát ứng dụng?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void frmSanPham_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDataChanged())
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có muốn lưu các thay đổi trước khi thoát?",
                    "Xác nhận lưu",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    btnLuu_Click(sender, e);  
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;  
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn thoát ứng dụng?",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true; 
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                using (QLSanPhamContextDB db = new QLSanPhamContextDB())
                {
                    var sp = db.Sanphams.FirstOrDefault(x => x.MaSP == txtMaSP.Text);

                    if (sp == null)
                    {
         
                        var newSanPham = new Sanpham
                        {
                            MaSP = txtMaSP.Text,
                            TenSP = txtTenSP.Text,
                            Ngaynhap = dtNgayNhap.Value,
                            MaLoai = cboLoaiSP.SelectedValue.ToString()
                        };

                        db.Sanphams.Add(newSanPham);
                    }
                    else
                    {
        
                        sp.TenSP = txtTenSP.Text;
                        sp.Ngaynhap = dtNgayNhap.Value;
                        sp.MaLoai = cboLoaiSP.SelectedValue.ToString();
                    }

                    db.SaveChanges();  
                    BindGrid(db.Sanphams.ToList());  

                    MessageBox.Show("Dữ liệu đã được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    btnLuu.Enabled = false;
                    btnKLuu.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu sản phẩm: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKLuu_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bạn có chắc chắn muốn hủy các thay đổi không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                btnLuu.Enabled = false;
                btnKLuu.Enabled = false;
                MessageBox.Show("Các thay đổi đã được hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CheckAndEnableSaveButtons()
        {
            if (IsDataChanged())
            {
                btnLuu.Enabled = true;
                btnKLuu.Enabled = true;
            }
            else
            {
                btnLuu.Enabled = false;
                btnKLuu.Enabled = false;
            }
        }

        private bool IsDataChanged()
        {
            QLSanPhamContextDB db = new QLSanPhamContextDB();
            var sp = db.Sanphams.FirstOrDefault(x => x.MaSP == txtMaSP.Text);

            if (sp == null)
            {
                return !string.IsNullOrWhiteSpace(txtMaSP.Text) ||
                       !string.IsNullOrWhiteSpace(txtTenSP.Text) ||
                       cboLoaiSP.SelectedIndex != -1;
            }

            return txtTenSP.Text != sp.TenSP ||
                   dtNgayNhap.Value.Date != sp.Ngaynhap.Date ||
                   cboLoaiSP.SelectedValue.ToString() != sp.MaLoai;
        }

        private void txtMaSP_TextChanged(object sender, EventArgs e)
        {
            CheckAndEnableSaveButtons();
        }

        private void txtTenSP_TextChanged(object sender, EventArgs e)
        {
            CheckAndEnableSaveButtons();
        }

        private void dtNgayNhap_ValueChanged(object sender, EventArgs e)
        {
            CheckAndEnableSaveButtons();
        }

        private void cboLoaiSP_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckAndEnableSaveButtons();
        }
    }
}
