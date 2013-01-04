#!/usr/bin/ruby

require 'tlsmail'
require 'time'

from = 'brady.holt@gmail.com'
to = ['brady.holt@gmail.com']

content = <<EOF
From: Brady Holt <#{from}>
To: #{to.join(", ")}
Subject: Citizen Patrol Reminder: Update spreadsheet for #{Date.today.strftime("%B")}
Date: #{Time.now.rfc2822}
MIME-Version: 1.0
Content-Type: text/html

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.or$
<html>
<head><title></title>
</head>
<body>
<p>The Citizen Patrol hours log reminder email will be sent on the 28th of this month.  The spreadsheet needs to be updated.  The link to the spreadsheet is listed below:</p>
<p><a href="https://docs.google.com/spreadsheet/ccc?key=0AumP8MJ6pPMwdG03WUQ0SEV6cm5tTklqRF95aWUtNVE#gid=1">https://docs.google.com/spreadsheet/ccc?key=0AumP8MJ6pPMwdG03WUQ0SEV6cm5tTklqRF95aWUtNVE#gid=1</a></p>
</body>
</html>
EOF

Net::SMTP.enable_tls(OpenSSL::SSL::VERIFY_NONE)
Net::SMTP.start('smtp.gmail.com', 587, 'gmail.com', from, '9796BmH9796', :login) do |smtp|
  smtp.send_message(content, '#{from}', to)
end
