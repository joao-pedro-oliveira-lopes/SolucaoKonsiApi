namespace KonsiApi.Models
{
    public class Beneficio
    {
        public int Id { get; set; }
        public string NumeroMatricula { get; set; }
        public string CodigoTipoBeneficio { get; set; }
        public Cpf Cpf { get; set; }
    }
}
