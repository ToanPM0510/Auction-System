using AuctionSystem.Models;
using AutoMapper;

namespace AuctionSystem.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Auction, AuctionDTO>();
            CreateMap<Bid, BidDTO>();
            CreateMap<AutoBid, AutoBidDTO>();
            CreateMap<User, UserDTO>();
        }
    }
}