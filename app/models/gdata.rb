class Gdata
  DOCUMENT_DOWNLOAD_PATH = "download/documents/export/Export"
  DOCUMENT_DOWNLOAD_KEY_PARAM = "id"
  SPREADHSEET_DOWNLOAD_PATH = "download/spreadsheets/Export"
  SPREADSHEET_DOWNLOAD_KEY_PARAM = "key"
  EXPORT_FORMAT_KEY_PARAM = "exportFormat"
  FORMAT_KEY_PARAM = "format"
  FORMAT_HTML = "html"
  FORMAT_HTML_BODY_ONLY = "htmlBody"

  @@username = ElfWeb::Application.config.gdata_username
  @@password = ElfWeb::Application.config.gdata_password

  def self.retrieveDocument(key, format)
    client = GData::Client::DocList.new
    docBody = retrieveGData(client, DOCUMENT_DOWNLOAD_PATH, DOCUMENT_DOWNLOAD_KEY_PARAM, key, format)

    if format == FORMAT_HTML_BODY_ONLY
      docBody = Gdata.extractHtmlBody(docBody)
    end

    return docBody
  end

  def self.retrieveSpreadSheet(key, format)
    client = GData::Client::Spreadsheets.new
    docBody = retrieveGData(client, SPREADHSEET_DOWNLOAD_PATH, SPREADSHEET_DOWNLOAD_KEY_PARAM, key, format)

    if format == FORMAT_HTML_BODY_ONLY
      docBody = Gdata.extractHtmlBody(docBody, "<table")
    end

    return docBody
  end

  def self.retrieveGData(client, downloadPath, keyParam, key, format)
    client.clientlogin(@@username, @@password)

    if format == FORMAT_HTML_BODY_ONLY
      format = FORMAT_HTML
    end

    donwloadPath = client.authsub_scope +  downloadPath + "?#{keyParam}=#{key}&#{EXPORT_FORMAT_KEY_PARAM}=#{format}&#{FORMAT_KEY_PARAM}=#{format}"
    doc = client.get(donwloadPath)
    return doc.body
  end

  def self.extractHtmlBody(fullHtml, bodyStartElement = nil)
      styleStartIndex  = fullHtml.index('<style')
      styleEndIndex = fullHtml.rindex('</style') + 8

      if (bodyStartElement.nil?)
        bodyStartIndex = fullHtml.index('<', fullHtml.index('<body') + 1)
      else
        bodyStartIndex = fullHtml.index(bodyStartElement)
      end

      bodyEndIndex = fullHtml.index('</body>')

      if (!bodyStartIndex.nil? && !bodyEndIndex.nil?)
        docBodyFormatted = fullHtml[bodyStartIndex, bodyEndIndex - bodyStartIndex]
      end

      if (!styleStartIndex.nil? && !styleEndIndex.nil?)
        style = fullHtml[styleStartIndex, styleEndIndex - styleStartIndex]
        style.gsub!("html{", "html_supressed{")
        style.gsub!("body{", "body_supressed{")
        style.gsub!("h1{", "h1_supressed{")
        style.gsub!("h2{", "h2_supressed{")
        style.gsub!("h3{", "h3_supressed{")
        style.gsub!("h4{", "h4_supressed{")
        style.gsub!("h5{", "h5_supressed{")
        docBodyFormatted = docBodyFormatted + style
      end

      if (docBodyFormatted.nil?)
           docBodyFormatted = "Error extracting HTML!"
      end
      return docBodyFormatted
  end
end