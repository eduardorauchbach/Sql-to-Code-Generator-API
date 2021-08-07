using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Helpers;

namespace WorkUtilities.Models
{
    /// <summary>
    /// Objeto de entrada padrão
    /// </summary>
	public class GeneratorModel
	{
        /// <summary>
        /// Nome do Projeto a ser atachado o código
        /// </summary>
		public string ProjectName { get; set; } = StringHelper.Unidentified;
        /// <summary>
        /// Se Logs estarão em uso na aplicação
        /// </summary>
        public bool IsLogsActive { get; set; } = true;
        /// <summary>
        /// Lista de Modelos a serem criados
        /// </summary>
		public List<EntryModel> EntryModels { get; set; }
    }

    public static class ProcessorGeneratorModel
    {
        public static void PreProcess(this GeneratorModel model)
        {
            //Todo: Fix Relation N to N
        }
    }
}
