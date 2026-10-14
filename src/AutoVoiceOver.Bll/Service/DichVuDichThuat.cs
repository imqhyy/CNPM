using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Bll.Service
{
	public class DichVuDichThuat : IDichVuDichThuat
	{
		private readonly HttpClient _boGoiHttp;

		public DichVuDichThuat(HttpClient boGoiHttp)
		{
			_boGoiHttp = boGoiHttp ?? throw new ArgumentNullException(nameof(boGoiHttp));
		}

		public async Task<string> DichVanBanAsync(string vanBan, string ngonNguGoc, string ngonNguDich)
		{	
			// văn bản rỗng hoặc chỉ có dấu cách thì báo lỗi
			if (string.IsNullOrWhiteSpace(vanBan)) 
				throw new ArgumentException("Văn bản cần dịch không được để trống.", nameof(vanBan));
			if (string.IsNullOrWhiteSpace(ngonNguGoc)) 
				throw new ArgumentException("Ngôn ngữ gốc không được để trống.", nameof(ngonNguGoc));
			if (string.IsNullOrWhiteSpace(ngonNguDich))
				throw new ArgumentException("Ngôn ngữ đích không được để trống.", nameof(ngonNguDich));

			// "vi-VN" -> "vi", "en-US" -> "en" (chuyển sang vi - en để API dịch)
			string goc = ChuanHoaMaNgonNgu(ngonNguGoc);
			string dich = ChuanHoaMaNgonNgu(ngonNguDich);

			// Cùng ngôn ngữ thì không cần gọi API (trả về văn bann gốc)
			if (goc == dich)
				return vanBan;

			string capNgonNgu = Uri.EscapeDataString($"{goc}|{dich}");
			string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(vanBan)}&langpair={capNgonNgu}";

			using HttpResponseMessage phanHoi = await _boGoiHttp.GetAsync(url); // gửi yêu cầu và kiểm tra phản hồi
			phanHoi.EnsureSuccessStatusCode();

			string noiDungJson = await phanHoi.Content.ReadAsStringAsync();
			using JsonDocument taiLieu = JsonDocument.Parse(noiDungJson);

			string? vanBanDaDich = null;
			if (taiLieu.RootElement.TryGetProperty("responseData", out JsonElement duLieu) && // lấy dữ liệu dịch
				duLieu.TryGetProperty("translatedText", out JsonElement banDich)) // lấy văn bản đã dịch
			{
				vanBanDaDich = banDich.GetString(); 
			}

			if (string.IsNullOrWhiteSpace(vanBanDaDich)) 
				throw new InvalidOperationException("Dịch vụ dịch thuật không trả về kết quả."); //nếu không có kết quả dịch thì báo lỗi

			return vanBanDaDich; // trả về văn bản đã dịch
		}

		private static string ChuanHoaMaNgonNgu(string maNgonNgu) 
		{
			string ma = maNgonNgu.Trim(); // loại bỏ khoảng trắng đầu và cuối
			int viTri = ma.IndexOf('-'); 
			if (viTri > 0)
				ma = ma[..viTri]; 
			return ma.ToLowerInvariant(); 
		}
	}
}
