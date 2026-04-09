using System;
using System.Collections.Generic;

namespace foodstreet_admin.Services
{
    public class UILanguageService
    {
        public string CurrentLanguage { get; private set; } = "vi";
        public event Action? OnLanguageChanged;

        private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            ["vi"] = new()
            {
                ["Dashboard"] = "Dashboard",
                ["MyStore"] = "Gian hàng của tôi",
                ["Stats"] = "Thống kê",
                ["Profile"] = "Cấu hình",
                ["Logout"] = "Đăng xuất",
                ["StoreName"] = "Tên gian hàng",
                ["Description"] = "Mô tả",
                ["Update"] = "Cập nhật",
                ["Delete"] = "Xóa",
                ["GenerateTTS"] = "Tạo thuyết minh",
                ["SelectLanguage"] = "Chọn ngôn ngữ",
                ["Hello"] = "Xin chào",
                ["Notifications"] = "Thông báo",
                ["Save"] = "Lưu lại",
                ["Cancel"] = "Hủy bỏ",
                ["Success"] = "Thành công",
                ["Error"] = "Lỗi",
                ["Category"] = "Danh mục",
                ["Location"] = "Vị trí",
                ["Menu"] = "Thực đơn",
                ["Items"] = "Món ăn",
                ["Price"] = "Giá",
                ["Action"] = "Thao tác",
                ["Add"] = "Thêm mới",
                ["Edit"] = "Sửa",
                ["Search"] = "Tìm kiếm",
                ["Generate"] = "Tạo audio",
                ["Processing"] = "Đang xử lý...",
                ["ConfirmDelete"] = "Bạn có chắc muốn xóa?",
                ["Language"] = "Ngôn ngữ",
                ["StoreSettings"] = "Cài đặt gian hàng",
                ["MonthlyStats"] = "Thống kê theo tháng",
                ["Month"] = "Tháng",
                ["Listens"] = "Lượt nghe",
                ["Views"] = "Lượt xem",
                ["Reviews"] = "Review"
            },
            ["en"] = new()
            {
                ["Dashboard"] = "Dashboard",
                ["MyStore"] = "My Store",
                ["Stats"] = "Analytics",
                ["Profile"] = "Settings",
                ["Logout"] = "Logout",
                ["StoreName"] = "Store Name",
                ["Description"] = "Description",
                ["Update"] = "Update",
                ["Delete"] = "Delete",
                ["GenerateTTS"] = "Generate TTS",
                ["SelectLanguage"] = "Select Language",
                ["Hello"] = "Hello",
                ["Notifications"] = "Notifications",
                ["Save"] = "Save",
                ["Cancel"] = "Cancel",
                ["Success"] = "Success",
                ["Error"] = "Error",
                ["Category"] = "Category",
                ["Location"] = "Location",
                ["Menu"] = "Menu",
                ["Items"] = "Items",
                ["Price"] = "Price",
                ["Action"] = "Action",
                ["Add"] = "Add New",
                ["Edit"] = "Edit",
                ["Search"] = "Search",
                ["Generate"] = "Generate Audio",
                ["Processing"] = "Processing...",
                ["ConfirmDelete"] = "Are you sure you want to delete?",
                ["Language"] = "Language",
                ["StoreSettings"] = "Store Settings",
                ["MonthlyStats"] = "Monthly Statistics",
                ["Month"] = "Month",
                ["Listens"] = "Listens",
                ["Views"] = "Views",
                ["Reviews"] = "Reviews"
            },
            ["zh"] = new()
            {
                ["Dashboard"] = "控制面板",
                ["MyStore"] = "我的店铺",
                ["Stats"] = "统计分析",
                ["Profile"] = "个人设置",
                ["Logout"] = "退出登录",
                ["StoreName"] = "店铺名称",
                ["Description"] = "描述",
                ["Update"] = "更新",
                ["Delete"] = "删除",
                ["GenerateTTS"] = "生成语音",
                ["SelectLanguage"] = "选择语言",
                ["Hello"] = "您好",
                ["Notifications"] = "通知",
                ["Save"] = "保存",
                ["Cancel"] = "取消",
                ["Success"] = "成功",
                ["Error"] = "错误",
                ["Category"] = "类别",
                ["Location"] = "地点",
                ["Menu"] = "菜单",
                ["Items"] = "项目",
                ["Price"] = "价格",
                ["Action"] = "操作",
                ["Add"] = "新增",
                ["Edit"] = "编辑",
                ["Search"] = "搜索",
                ["Generate"] = "生成音频",
                ["Processing"] = "处理中...",
                ["ConfirmDelete"] = "您确定要删除吗？",
                ["Language"] = "语言",
                ["StoreSettings"] = "店铺设置",
                ["MonthlyStats"] = "每月统计",
                ["Month"] = "月份",
                ["Listens"] = "收听次数",
                ["Views"] = "浏览次数",
                ["Reviews"] = "评论"
            }
        };

        public string T(string key)
        {
            if (_translations.ContainsKey(CurrentLanguage) && _translations[CurrentLanguage].ContainsKey(key))
            {
                return _translations[CurrentLanguage][key];
            }
            return key;
        }

        public void SetLanguage(string lang)
        {
            if (CurrentLanguage != lang)
            {
                CurrentLanguage = lang;
                OnLanguageChanged?.Invoke();
            }
        }
    }
}
