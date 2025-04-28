using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Convert
{
    public class Tools
    {
        /// <summary>
        /// 带符号的数字字符串转换成数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveLeadingZeros(string input)
        {
            //检查是否含有+、-符号
            if (input.Contains("+") || input.Contains("-")){
                string tag = input.Substring(0, 1);
                input = input.Substring(1);
                string outPut = removeLeadingZeros(input);
                return $"{tag}{outPut}";
            }
            else
            {
                return removeLeadingZeros(input);
            }

           
        }

        /// <summary>
        /// 不带符号的数字字符串转换成数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string removeLeadingZeros(string input)
        {
            // 先检查是否为纯数字或者包含小数点
            if (input.Contains('.'))
            {
                // 分割整数部分和小数部分
                string[] parts = input.Split('.');
                string integerPart = parts[0].TrimStart('0');
                string decimalPart = parts[1];

                // 如果整数部分为空，说明整数部分全是 0，置为 "0"
                if (string.IsNullOrEmpty(integerPart))
                {
                    integerPart = "0";
                }

                // 重新组合整数部分和小数部分
                return $"{integerPart}.{decimalPart}";
            }
            else
            {
                // 不包含小数点，使用原逻辑
                string result = input.TrimStart('0');
                return string.IsNullOrEmpty(result) ? "0" : result;
            }
        } 

    }
}
