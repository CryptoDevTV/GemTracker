﻿using GemTracker.Shared.Domain.DTOs;
using GemTracker.Shared.Domain.Enums;
using GemTracker.Shared.Domain.Models;
using GemTracker.Shared.Domain.Statics;
using GemTracker.Shared.Extensions;
using GemTracker.Shared.Services;
using GemTracker.Shared.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GemTracker.Shared.Domain
{
    public class Kyber
    {
        private readonly IKyberService _kyberService;
        private readonly IFileService _fileService;

        public string StorageFilePath { get; private set; }
        public string StorageFilePathDeleted { get; private set; }
        public string StorageFilePathAdded { get; private set; }
        public Kyber(IKyberService kyberService,
            IFileService fileService)
        {
            _kyberService = kyberService;
            _fileService = fileService;
        }
        public void SetPaths(string storagePath)
        {
            StorageFilePath = PathTo.All(DexType.KYBER, storagePath);
            StorageFilePathDeleted = PathTo.Deleted(DexType.KYBER, storagePath);
            StorageFilePathAdded = PathTo.Added(DexType.KYBER, storagePath);
        }
        public async Task<KyberTokensResponse> FetchAllAsync()
        {
            var result = new KyberTokensResponse();

            var response = await _kyberService.FetchAllAsync();

            if (response.Success)
            {
                result.List = response.List;
            }
            else
            {
                result.Message = response.Message;
            }
            return result;
        }
        public async Task<Loaded<KyberToken>> LoadAllAsync()
        {
            var result = new Loaded<KyberToken>();
            try
            {
                result.OldList = await _fileService.GetAsync<IEnumerable<KyberToken>>(StorageFilePath);
                result.OldListDeleted = await _fileService.GetAsync<List<Gem>>(StorageFilePathDeleted);
                result.OldListAdded = await _fileService.GetAsync<List<Gem>>(StorageFilePathAdded);
            }
            catch (Exception ex)
            {
                result.Message = ex.GetFullMessage();
            }
            return result;
        }
        public IEnumerable<Gem> CheckDeleted(IEnumerable<Token> oldList, IEnumerable<Token> newList, TokenActionType tokenActionType)
        {
            var recentlyDeleted = new List<Gem>();

            var deletedFromDex = oldList
                    .Where(p => newList
                    .All(p2 => p2.Id != p.Id))
                    .ToList();

            if (deletedFromDex.Count() > 0)
            {
                foreach (var item in deletedFromDex)
                {
                    var gem = new Gem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Symbol = item.Symbol,
                        Recently = tokenActionType,
                        Date = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        IsPublished = false
                    };
                    recentlyDeleted.Add(gem);
                }
            }
            return recentlyDeleted;
        }

        public IEnumerable<Gem> CheckAdded(IEnumerable<Token> oldList, IEnumerable<Token> newList, TokenActionType tokenActionType)
        {
            var recentlyAdded = new List<Gem>();

            var addedToDex = newList
                    .Where(p => oldList
                    .All(p2 => p2.Id != p.Id))
                    .ToList();

            if (addedToDex.Count() > 0)
            {
                foreach (var item in addedToDex)
                {
                    var gem = new Gem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Symbol = item.Symbol,
                        Recently = tokenActionType,
                        Date = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        IsPublished = false
                    };
                    recentlyAdded.Add(gem);
                }
            }
            return recentlyAdded;
        }
    }
}