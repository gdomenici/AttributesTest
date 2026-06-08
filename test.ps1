$body = @'
<?xml version="1.0" encoding="utf-8"?>
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <Hello xmlns="http://tempuri.org/">
      <name>World</name>
    </Hello>
  </s:Body>
</s:Envelope>
'@

Invoke-WebRequest -Uri "http://localhost:5000/MyService.svc" `
    -Method POST `
    -ContentType "text/xml; charset=utf-8" `
    -Headers @{ SOAPAction = "http://tempuri.org/IMyService/Hello" } `
    -Body $body `
    | Select-Object -ExpandProperty Content