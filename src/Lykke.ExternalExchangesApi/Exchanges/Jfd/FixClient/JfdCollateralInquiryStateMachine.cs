﻿using System;
using System.Collections.Generic;
using System.Threading;
using Common.Log;
using Lykke.ExternalExchangesApi.Shared;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.ExternalExchangesApi.Exchanges.Jfd.FixClient
{
    internal sealed class JfdCollateralInquiryStateMachine : RequestStateMachine<IReadOnlyCollection<CollateralReport>>
    {
        private bool _ackReceived;
        private int _promisedReports;
        private readonly List<CollateralReport> _positions = new List<CollateralReport>();

        public JfdCollateralInquiryStateMachine(CollateralInquiry positionsRequest, CancellationToken cancellationToken, IMessageHandler handler, ILog log) : base(positionsRequest, cancellationToken, handler, log)
        {
            positionsRequest.CollInquiryID = new CollInquiryID(Id);
        }

        public void ProcessResponse(CollateralInquiryAck message)
        {
            if (_ackReceived)
            {
                Log.WriteWarningAsync(nameof(ProcessResponse), "Handling response from Jfd", $"Unexpected RequestForPositionsAck received. Id {message.CollInquiryID.Obj}").GetAwaiter().GetResult();
            }
            _ackReceived = true;

            if (message.CollInquiryResult.Obj == CollInquiryResult.OTHER)
            {
                var msg = message.IsSetText() ? message.Text.Obj : "Position request rejected. No additional information";
                TaskCompletionSource.SetException(new InvalidOperationException(msg));
                Status = RequestStatus.Completed;
            }
            _promisedReports = message.TotNumReports.Obj;
            if (_promisedReports == 0)
            {
                TaskCompletionSource.TrySetResult(Array.Empty<CollateralReport>());
                Status = RequestStatus.Completed;
            }
            else
            {
                Status = RequestStatus.InProgress;
            }
        }

        public void ProcessResponse(CollateralReport message)
        {
            _positions.Add(message);
            if (_positions.Count == _promisedReports)
            {
                TaskCompletionSource.TrySetResult(_positions);
                Status = RequestStatus.Completed;
            }
        }
    }
}
