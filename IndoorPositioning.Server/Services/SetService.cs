﻿using IndoorPositioning.Server.Clients;
using IndoorPositioning.Server.Database.Dao;
using IndoorPositioning.Server.Database.Model;
using System.Text;

namespace IndoorPositioning.Server.Services
{
    public class SetService : BaseService, IService
    {
        public SetService(ServiceClient serviceClient) : base(serviceClient) { }

        public void Service(string data)
        {
            string inData = data.ToLower();
            string[] dataItems = inData.Split(' ');

            /* Check data */
            if (dataItems.Length < 2)
            {
                ServiceClient.Send(INVALID_PARAMETERS_ERROR);
                return;
            }

            string command = dataItems[1];

            /* Determine the command */
            if ("mode".Equals(command)) SetMode(dataItems);
            else ServiceClient.Send(UNKNOWN_COMMAND_ERROR);
        }

        /* Creates error message for set mode command */
        private string CreateError_SetMode()
        {
            /* Create error message */
            StringBuilder sb = new StringBuilder()
                .AppendLine(INVALID_PARAMETERS_ERROR)
                .AppendLine("Sample: set mode positioning")
                .AppendLine("Sample: set mode fingerprinting -env 2 -beacon 3 -x 10 -y 20")
                .AppendLine("-env: environment id to be fingerprinted")
                .AppendLine("-beacon: beacon id to be used for fingerprinting")
                .AppendLine("-x: x axis")
                .AppendLine("-y: y axis");
            foreach (var item in Server.Modes)
            {
                sb.AppendLine(string.Format($"{item.Key} for {item.Value}"));
            }

            return sb.ToString();
        }

        private int GetIndex(string[] dataItems, string p)
        {
            for (int i = 0; i < dataItems.Length; i++)
            {
                if (dataItems[i].Equals(p)) return i;
            }
            return -1;
        }

        /* Updates the beacon that is provided as json */
        private void SetMode(string[] dataItems)
        {
            /* Check data */
            if (dataItems.Length < 3)
            {
                ServiceClient.Send(CreateError_SetMode());
                return;
            }

            string mode = dataItems[2];

            /* If mode string is invalid */
            if (!Server.Modes.ContainsKey(mode))
            {
                ServiceClient.Send(CreateError_SetMode());
                return;
            }

            /* If fingerprinting mode is being tried to set */
            if (Server.Modes[mode] == Enums.ServerModes.Fingerprinting)
            {
                /* Check the command whether it is valid for the fingerprinting */
                if (dataItems.Length < 11)
                {
                    ServiceClient.Send(CreateError_SetMode());
                    return;
                }

                /* Get coordinates */
                int envIndex = GetIndex(dataItems, "-env");
                if (envIndex == -1)
                {
                    ServiceClient.Send(CreateError_SetMode());
                    return;
                }
                int beaconIndex = GetIndex(dataItems, "-beacon");
                if (beaconIndex == -1)
                {
                    ServiceClient.Send(CreateError_SetMode());
                    return;
                }
                int xIndex = GetIndex(dataItems, "-x");
                if (xIndex == -1)
                {
                    ServiceClient.Send(CreateError_SetMode());
                    return;
                }
                int yIndex = GetIndex(dataItems, "-y");
                if (yIndex == -1)
                {
                    ServiceClient.Send(CreateError_SetMode());
                    return;
                }
                /* Exceptions will be handled by ServiceClient */
                int env = int.Parse(dataItems[envIndex + 1]);
                int beaconId = int.Parse(dataItems[beaconIndex + 1]);
                int x = int.Parse(dataItems[xIndex + 1]);
                int y = int.Parse(dataItems[yIndex + 1]);

                /* Get the mac address of the beacon provided! */
                BeaconDao dao = new BeaconDao();
                Beacon beacon = dao.GetBeaconById(beaconId);

                /*Set coordinates*/
                Server.Fingerprinting_EnvironmentId = env;
                Server.Fingerprinting_BeaconId = beaconId;
                Server.Fingerprinting_BeaconMacAddress = beacon.MACAddress;
                Server.Fingerprinting_X = x;
                Server.Fingerprinting_Y = y;
            }

            /* Set mode */
            Server.ServerMode = Server.Modes[mode];
            ServiceClient.Send(OK);
        }
    }
}
