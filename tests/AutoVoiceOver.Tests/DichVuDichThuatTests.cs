using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using AutoVoiceOver.Bll.Service;

namespace AutoVoiceOver.Tests;

public class DichVuDichThuatTests
{
	// HttpMessageHandler giả: không gọi mạng thật, chỉ trả về nội dung định sẵn
	private sealed class BoXuLyGia : HttpMessageHandler
	{
		private readonly HttpStatusCode _maTrangThai;
		private readonly string _noiDung;

		public int SoLanGoi { get; private set; }
		public Uri? UrlDaGoi { get; private set; }

		public BoXuLyGia(HttpStatusCode maTrangThai, string noiDung) 
		{
			_maTrangThai = maTrangThai;
			_noiDung = noiDung;
		}

		protected override Task<HttpResponseMessage> SendAsync( // Ghi đè phương thức SendAsync để trả về phản hồi giả
			HttpRequestMessage request, CancellationToken cancellationToken) 
		{
			SoLanGoi++;
			UrlDaGoi = request.RequestUri;
			return Task.FromResult(new HttpResponseMessage(_maTrangThai) // trả về phản hồi giả với mã trạng thái và nội dung JSON định sẵn
			{
				Content = new StringContent(_noiDung, Encoding.UTF8, "application/json") 
			});
		}
	}

	private static (DichVuDichThuat dichVu, BoXuLyGia boXuLy) TaoDichVu( // Tạo dịch vụ dịch thuật với HttpClient giả
		HttpStatusCode maTrangThai, string noiDung)
	{
		var boXuLy = new BoXuLyGia(maTrangThai, noiDung);
		var dichVu = new DichVuDichThuat(new HttpClient(boXuLy));
		return (dichVu, boXuLy);
	}

	private const string JsonThanhCong =
		"{\"responseData\":{\"translatedText\":\"Xin chào\"},\"responseStatus\":200}"; // JSON giả trả về khi dịch thành công

	[Fact]
	public async Task DichVanBanAsync_ApiThanhCong_TraVeBanDich() // Kiểm tra dịch văn bản thành công
	{
		var (dichVu, _) = TaoDichVu(HttpStatusCode.OK, JsonThanhCong); 

		string ketQua = await dichVu.DichVanBanAsync("Hello", "en", "vi"); // dịch từ tiếng Anh sang tiếng Việt

		Assert.Equal("Xin chào", ketQua); 
	}

	[Fact]
	public async Task DichVanBanAsync_CungNgonNgu_TraVanBanGocVaKhongGoiApi() // Kiểm tra khi cùng ngôn ngữ thì trả về văn bản gốc và không gọi API
	{
		var (dichVu, boXuLy) = TaoDichVu(HttpStatusCode.OK, JsonThanhCong);

		string ketQua = await dichVu.DichVanBanAsync("Xin chào", "vi-VN", "vi"); // dịch từ tiếng Việt sang tiếng Việt

		Assert.Equal("Xin chào", ketQua);
		Assert.Equal(0, boXuLy.SoLanGoi);
	}

	[Fact]
	public async Task DichVanBanAsync_MaLocale_DuocChuanHoaThanhMaNgan() // Kiểm tra khi mã ngôn ngữ có dạng locale (ví dụ "en-US") thì được chuẩn hóa thành mã ngắn (ví dụ "en")
	{
		var (dichVu, boXuLy) = TaoDichVu(HttpStatusCode.OK, JsonThanhCong); 

		await dichVu.DichVanBanAsync("Hello", "en-US", "vi-VN"); 

		Assert.NotNull(boXuLy.UrlDaGoi);
		Assert.Contains("langpair=en%7Cvi", boXuLy.UrlDaGoi!.AbsoluteUri); 
	}

	[Theory] 
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")] 
	public async Task DichVanBanAsync_VanBanTrong_NemArgumentException(string? vanBan) // Kiểm tra khi văn bản cần dịch là null, rỗng hoặc chỉ có khoảng trắng thì ném ArgumentException
	{
		var (dichVu, _) = TaoDichVu(HttpStatusCode.OK, JsonThanhCong); 

		await Assert.ThrowsAsync<ArgumentException>( 
			() => dichVu.DichVanBanAsync(vanBan!, "en", "vi")); 
	}

	[Fact]
	public async Task DichVanBanAsync_ApiLoi500_NemHttpRequestException() // Kiểm tra khi API trả về lỗi 500 thì ném HttpRequestException
	{
		var (dichVu, _) = TaoDichVu(HttpStatusCode.InternalServerError, "{}"); 

		await Assert.ThrowsAsync<HttpRequestException>( 
			() => dichVu.DichVanBanAsync("Hello", "en", "vi")); 
	}

	[Fact]
	public async Task DichVanBanAsync_JsonKhongCoBanDich_NemInvalidOperationException() // Kiểm tra khi JSON trả về không có văn bản dịch thì ném InvalidOperationException
	{
		var (dichVu, _) = TaoDichVu(HttpStatusCode.OK, "{\"responseStatus\":200}"); // JSON không có "responseData" hoặc "translatedText"	

		await Assert.ThrowsAsync<InvalidOperationException>( 
			() => dichVu.DichVanBanAsync("Hello", "en", "vi"));
	}
}
