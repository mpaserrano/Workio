namespace Workio.Models
{
    /// <summary>
    /// Enumerado que representa o estado de uma denúncia.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Estado de denuncia que representa que foi vista mas rejeitada.
        /// </summary>
        Rejected,
        /// <summary>
        /// Estado de denuncia que representa que está por ser resolvida.
        /// </summary>
        Pending,
        /// <summary>
        /// Estado de denuncia que representa que foi vista e aceite, o/a reportado/a foi banido/a.
        /// </summary>
        Accepted
    }
}
