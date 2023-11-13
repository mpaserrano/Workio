namespace Workio.Models
{
    /// <summary>
    /// Enumerado que representa o estado de uma conexão.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Estado da conexão que representa que foi aceite
        /// </summary>
        Accepted,
        /// <summary>
        /// Estado da conexão que representa que ainda não foi aceite ou rejeitada.
        /// </summary>
        Pending
    }
}
