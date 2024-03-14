using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

public class UpstreamProxyTraceHeaderTelemetryInitializer : TelemetryInitializerBase
    {
        public ICollection<string> HeaderNames { get; }

        public UpstreamProxyTraceHeaderTelemetryInitializer(IHttpContextAccessor httpContextAccessor, ICollection<string>? headerNames = default(ICollection<string>))
             : base(httpContextAccessor)
        {
            if ( headerNames == null){
                headerNames =new string[] { "x-azure-ref", "x-appgw-trace-id"};
            }
            HeaderNames = headerNames;
        }

        protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
        {

            requestTelemetry.Context.Cloud.RoleName = "DoSomething Service";

            if ( null == this.HeaderNames || this.HeaderNames.Count==0) {
                    return;
            }
            if (telemetry == null)  {
                throw new ArgumentNullException(nameof(telemetry));
            }
            if (requestTelemetry == null) {
                throw new ArgumentNullException(nameof(requestTelemetry));
            }
            if ( telemetry== requestTelemetry ) { // only do this for the request telemetry 
                if (platformContext == null) {
                    throw new ArgumentNullException(nameof(platformContext));
                }                 
                if (platformContext.Request?.Headers != null && platformContext.Request?.Headers.Count> 0) {
                    foreach (var name in this.HeaderNames) {
                        var value = GetHeaderValue(name,platformContext.Request.Headers);

                        if ( !String.IsNullOrEmpty(value)) {
                            AddReference(requestTelemetry,name, value);
                        }
                    }
                }
            }
        }

        private string GetHeaderValue(string headerNameToSearch, IHeaderDictionary requestHeaders){
            if ( null == requestHeaders || requestHeaders.Count== 0){
                 return string.Empty;
            }
            if (requestHeaders.ContainsKey(headerNameToSearch)) {
                return requestHeaders[headerNameToSearch];
            }
            return string.Empty;
        }     
        private void AddReference(RequestTelemetry requestTelemetry, string headerName, string headerValue){
            if ( null == requestTelemetry ){
                return;
            }
            requestTelemetry.Properties.Add(headerName, headerValue);
        }

}