using System.Runtime.InteropServices;
using System;
using RestSharp;

namespace BusboardCSharp {
    public static class TflClient {
        private static readonly RestClient StopPointClient = new RestClient("https://api.tfl.gov.uk/StopPoint");

        public static async Task<BusboardCSharp.StopPointArrival[]> getStopPointArrivals(){
            var request = new RestRequest($"{StopPointID.GetUserStopPointId()}/Arrivals");
            var response = await StopPointClient.GetAsync<BusboardCSharp.StopPointArrival[]>(request);

            if (response == null) {
                throw new Exception("Tfl API error");
            }
            return response;
        } 

        public static async Task<BusboardCSharp.StopPoint[]> getNearestStops(){
            var GeoLocationData = await GeoClient.GetGeolocation();
            var request = new RestRequest($"/?lat={GeoLocationData.Result.Latitude}&lon={GeoLocationData.Result.Longitude}&stopTypes=NaptanPublicBusCoachTram&radius=1000");
            var response = await StopPointClient.GetAsync<BusboardCSharp.StopPointData>(request);
            var modifiedresponse= new StopPoint[2];

            if (response == null) {
                throw new Exception("Tfl API error");

            } else if(response.StopPoints.Length==0){
                Console.WriteLine("No bus stops found within 1 km of your postcode.");
                Environment.Exit(0);
    
            } else if (response.StopPoints.Length>2){
                var SortbyDistanceResponse = response.StopPoints.OrderBy(s=>s.Distance).ToArray();
                 Array.Copy(SortbyDistanceResponse, 0, modifiedresponse, 0,2);
            } else {
                modifiedresponse = response.StopPoints;
            }


            return modifiedresponse;
        } 

        public static async Task<List<BusboardCSharp.Nearest_StopPoints>> getArrivalsfromPostCode(){
            List<Nearest_StopPoints> myResponse = new List<Nearest_StopPoints>();
           
            var Stops = await TflClient.getNearestStops();
            foreach (var Stop in Stops) {
                var request = new RestRequest($"{Stop.ID}/Arrivals");
                var response = await StopPointClient.GetAsync<BusboardCSharp.StopPointArrival[]>(request);

                if(response !=null && Stop.Modes !=null && Stop.Name !=null){
                    Nearest_StopPoints myObject = new Nearest_StopPoints(){
                        Modes = Stop.Modes,
                        Name = Stop.Name,
                        Distance = Stop.Distance,
                        Arrivals = response,
                    };
                    if(myObject.Arrivals.Length==0){
                        myObject.Message = "No buses arriving at this stop";
                    }
                    
                    myResponse.Add(myObject); 
                }

                if(myResponse[0].Arrivals.Length ==0 && myResponse[1].Arrivals.Length ==0){

                }
                        
            }
            

            return myResponse;
        }
    }
}

   