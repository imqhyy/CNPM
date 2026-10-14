using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoVoiceOver.Bll.Interface
{
	public interface IDichVuDichThuat //để bên thuyết minh gọi được
	{
		/// <summary>
		/// Dịch văn bản từ ngôn ngữ gốc sang ngôn ngữ đích.
		/// </summary>
		/// <param name="vanBan">Văn bản cần dịch</param>
		/// <param name="ngonNguGoc">Mã ngôn ngữ gốc, ví dụ "en"</param>
		/// <param name="ngonNguDich">Mã ngôn ngữ đích, ví dụ "vi"</param>
		/// <returns>Văn bản đã dịch</returns>
		Task<string> DichVanBanAsync(string vanBan, string ngonNguGoc, string ngonNguDich); 
	}
}