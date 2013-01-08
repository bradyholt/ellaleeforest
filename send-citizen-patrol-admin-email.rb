#!/usr/bin/ruby

require 'tlsmail'
require 'time'

from = 'brady.holt@gmail.com'
to = ['brady.holt@gmail.com']

content = <<EOF
From: Brady Holt <#{from}>
To: #{to.join(", ")}
Subject: Citizen Patrol Reporting - #{Date.today.strftime("%B")}
Date: #{Time.now.rfc2822}
MIME-Version: 1.0
Content-Type: text/html

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.or$
<html>
<head><title></title>
</head>
<body>
<ol>
<li>The Citizen Patrol hours log reminder email will be sent on the 28th of this month.  The spreadsheet needs to be updated.  Here is the link to update: <a href="https://docs.google.com/spreadsheet/ccc?key=0AumP8MJ6pPMwdG03WUQ0SEV6cm5tTklqRF95aWUtNVE#gid=1">https://docs.google.com/spreadsheet/ccc?key=0AumP8MJ6pPMwdG03WUQ0SEV6cm5tTklqRF95aWUtNVE#gid=1</a></li>
<li>The Citizen Patrol Report for <strong>#{(Date.today << 1).strftime("%B")}</strong> need to be submitted to HPD.  To generate the report, follow this link: <a href="http://www.ellaleeforest.org/citizen-patrol-report">http://www.ellaleeforest.org/citizen-patrol-report</a> and email to <a href="mailto:frank.escobedo@cityofhouston.net?subject=Citizen Patrol Report - #{(Date.today << 1).strftime("%B, %Y")}&body=Attached is the Ella Lee Forest Citizen Patrol Report for #{(Date.today << 1).strftime("%B, %Y")}.%0D%0A%0D%0AThanks,%0D%0ABrady Holt%0D%0AElla Lee Forest Citizen Patrol Coordinator">frank.escobedo@cityofhouston.net</a>.</li>
</ol>
</body>
</html>
EOF

Net::SMTP.enable_tls(OpenSSL::SSL::VERIFY_NONE)
Net::SMTP.start('smtp.gmail.com', 587, 'gmail.com', from, '9796BmH9796', :login) do |smtp|
  smtp.send_message(content, '#{from}', to)
end
