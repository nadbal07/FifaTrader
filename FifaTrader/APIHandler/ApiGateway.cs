﻿using FifaTrader.APIHandler.Interfaces;
using FifaTrader.Models;
using FifaTrader.Models.ModelBuilders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FifaTrader.APIHandler
{
    public class ApiGateway : IApiGateway
    {
        private readonly IGetRequestHandler _getRequestHandler;
        private readonly IBidViewModelBuilder _modelBuilder;
        private readonly IPutRequestHandler _putRequestHandler;
        private readonly IPostRequestHandler _postRequestHandler;
        private readonly IDeleteHandler _deleteHandler;

        public ApiGateway(IGetRequestHandler getRequestHandler, IBidViewModelBuilder modelBuilder,
            IPutRequestHandler putRequestHandler, IPostRequestHandler postRequestHandler,
            IDeleteHandler deleteHandler)
        {
            _getRequestHandler = getRequestHandler;
            _modelBuilder = modelBuilder;
            _putRequestHandler = putRequestHandler;
            _postRequestHandler = postRequestHandler;
            _deleteHandler = deleteHandler;
        }

        public async Task<string> BidOnPlayer(string tradeId, int bidPrice, string accessToken)
        {
            var bidResponse = await _putRequestHandler.PutBidOnPlayer(tradeId, bidPrice, accessToken);
            return bidResponse;
        }

        public async Task<string> CheckToken(string accessToken)
        {
            var response = await _getRequestHandler.CheckToken(accessToken);
            return response;
        }

        public async Task ClearExpiredPlayers(string accessToken, List<BidViewModel> allPlayers)
        {
            await _deleteHandler.DeleteExpiredPlayers(accessToken, allPlayers);
        }

        public async Task<List<BidViewModel>> FetchPlayers(int playerId, int bidPrice, string accessToken)
        {
            var searchList = await _getRequestHandler.SearchForSpecificPlayer(playerId, bidPrice, accessToken);
            return searchList;
        }

        public async Task<List<BidViewModel>> GetTransferTargets(string accessToken)
        {
            try
            {
                var targetsList = await _getRequestHandler.GetTransferTargets(accessToken);
                if(targetsList.Count == 0)
                {
                    throw new Exception();
                }

                var players = _modelBuilder.PopulateDefaultFieldsOfBidViews(targetsList);
                return players;
            }
            catch (Exception)
            {
                return new List<BidViewModel>
                {
                    new BidViewModel
                    {
                        Status = "Expired",
                        Pending = false,
                    }
                };
            }
        }

        public async Task<string> SellPlayer(string tradeId, string playerId, string accessToken, int startPrice, int BinPrice)
        {
            var moveCode = await _putRequestHandler.MovePlayerToTradePile(tradeId, playerId, accessToken);
            if(moveCode == HttpStatusCode.OK)
            {
                //Sell player
                var response = await _postRequestHandler.SellPlayer(playerId, accessToken, startPrice, BinPrice);

                return response;
            }

            return "Error";
        }
    }
}
