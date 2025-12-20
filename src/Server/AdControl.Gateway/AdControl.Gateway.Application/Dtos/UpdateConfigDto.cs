using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdControl.Gateway.Application.Dtos
{
    /// <summary>
    /// DTO для обновления полей конфига
    /// </summary>
    public class UpdateConfigDto
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Количество экранов, на которых отображается конфиг
        /// </summary>
        public int ScreensCount { get; set; }
        public bool IsStatic { get; set; }
    }
}
