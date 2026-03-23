using System;
using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using System.Threading.Tasks;
using Abp.UI;
using pbt.Entities;
using System.Linq;
using System.Net.Http;
using System.Text;
using Abp.Linq.Extensions;
using FuzzySharp;
using Newtonsoft.Json.Linq;
using pbt.Dictionary.Dto;

namespace pbt.Dictionary
{
    public class DictionaryAppService : AsyncCrudAppService<pbt.Entities.Dictionary, DictionaryDto, int, PagedDictionaryResultRequestDto, CreateUpdateDictionaryDto, DictionaryDto>, IDictionaryAppService
    {

        public DictionaryAppService(IRepository<pbt.Entities.Dictionary, int> repository)
            : base(repository)
        {

        }

        protected override IQueryable<pbt.Entities.Dictionary> CreateFilteredQuery(PagedDictionaryResultRequestDto input)
        {
            var query = Repository.GetAll()
               .WhereIf(!string.IsNullOrEmpty(input.Keyword), u => u.NameVi.Contains(input.Keyword) || u.NameCn.Contains(input.Keyword));
            return query;
        }

        /// <summary>
        /// Finds the dictionary entry with the closest match to the given Chinese name.
        /// If the similarity is less than 80, uses an external translation service (LibreTranslate).
        /// </summary>
        /// <param name="cnName">The Chinese name to search for.</param>
        /// <returns>The translated string or the closest matching entry's name.</returns>
        public async Task<string> GetDictionaryByCnName(string cnName)
        {
            if (string.IsNullOrEmpty(cnName))
            {
                return string.Empty;
            }

            var allDictionaries = await Repository.GetAllListAsync();
            var bestMatch = allDictionaries
                .OrderByDescending(d => Fuzz.Ratio(cnName, d.NameCn))
                .FirstOrDefault();

            // if (bestMatch == null)
            // {
            //     return await TranslateWithLingva(cnName);
            // }
            //
            // int similarityScore = Fuzz.Ratio(cnName, bestMatch.NameCn);
            // if (similarityScore < 60)
            // {
            //     return (await TranslateWithLingva(cnName)) ?? bestMatch.NameCn;
            // }
            return bestMatch?.NameVi;
        }

        // private async Task<string> TranslateWithLingva(string text)
        // {
        //     var encodedText = Uri.EscapeDataString(text);
        //     var apiUrl = $"https://lingva.ml/api/v1/zh/vi/{encodedText}";
        //     using (var client = new HttpClient())
        //     {
        //         try
        //         {
        //             // Thêm User-Agent để tránh bị chặn
        //             client.DefaultRequestHeaders.Add("User-Agent", 
        //                 "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        //             var response = await client.GetStringAsync(apiUrl);
        //             // Parse JSON
        //             dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
        //             string translatedText = result?.translation;
        //             return translatedText;
        //         }
        //         catch (Exception ex)
        //         {
        //             return null;
        //         }
        //     }
        // }

        
    }
}
