namespace HolidayApi.Controllers
{
    /// <summary>
    /// API válasz
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Sikeres volt-e a kérés?
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// A kért adat
        /// </summary>
        public T Data { get; set; }
    }
}
