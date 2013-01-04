#!/usr/bin/ruby

require 'tlsmail'
require 'time'

from = 'brady.holt@gmail.com'
to = ['ellaleeforest-citizenpatrol@googlegroups.com',
      'hthibaut1@comcast.net',
      'registration@jamf.us',
      'MarkertS@uhd.edu',
      'jtcalkins@sbcglobal.net',
      'brady.holt@gmail.com']

content = <<EOF
From: Brady Holt <#{from}>
To: #{to.join(", ")}
Subject: Citizen Patrol Reminder: #{Date.today.strftime("%B")} patrol hours
Date: #{Time.now.rfc2822}
MIME-Version: 1.0
Content-Type: text/html

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.or$
<html>
<head><title></title>
</head>
<body>

<p><strong>ELF Citizen Patrol,</strong></p>
<p>This is a friendly reminder to update the Patrol Reporting Log for the month of #{Date.today.strftime("%B")}.
<p>The link to the log can be found at the top of the Citizen Patrol webpage: <a href="http://www.ellaleeforest.org/citizen-patrol">http://www.ellaleeforest.org/citizen-patrol</a>.  If it prompts you for a password, it is <strong>elfhouston</strong>.</p>

<p>Thanks!<br/><br/>
Brady<br/>
ELF Citizen Patrol Coordinator<br/>
713-494-9132<br/>
<a href="mailto:#{from}">#{from}</a><br/>
</p>
</body>
</html>
EOF

Net::SMTP.enable_tls(OpenSSL::SSL::VERIFY_NONE)
Net::SMTP.start('smtp.gmail.com', 587, 'gmail.com', from, '9796BmH9796', :login) do |smtp|
  smtp.send_message(content, '#{from}', to)
end
