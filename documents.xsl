<?xml version="1.0" encoding="utf-8" ?>

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:fo="http://www.w3.org/1999/XSL/Format"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:xelby="http://www.sitebuilder.ru"
	xmlns:csxelby="http://www.spellabs.ru"
	xmlns:user="xelby"
	xmlns:csuser="csxelby"
	exclude-result-prefixes="fo xelby user msxsl">

  <xsl:output method="html" encoding="utf-8"/>
  <msxsl:script language="JScript" implements-prefix="user">
    <![CDATA[
		function getNameUrl(name)
		{
			return name.replace(" ","%20").replace("/","%2F");
		}
		function getCountUrl(url)
		{
			return url.split('/').length;
		}
		
	]]>
  </msxsl:script>
  <msxsl:script language="C#" implements-prefix="csuser">
    <![CDATA[
			
		]]>
  </msxsl:script>

  <xsl:variable name="EMPTY" select="''"/>

  <xsl:template match="/">
    <div class="m-site-hierarchy-menu">
      <div class="big-boss">
       <div class="title">
		    <a style="cursor:pointer">
			 <xsl:attribute name="onclick">
			  changeUrl('RootFolder=%2FShared%20Documents%2F')
			 </xsl:attribute>
			 Документы
			</a>
       </div>
      </div>
      <xsl:apply-templates select="//item">
        <xsl:with-param name="level" select="2"/>
        <xsl:with-param name="ParentUrl" select="string('Shared Documents/')"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="//item">
    <xsl:param name="level"/>
    <xsl:param name="ParentUrl"/>
    <xsl:if test="user:getCountUrl(string(@url))=$level and contains(@url,$ParentUrl) and @ct='Папка'">
      <xsl:variable name="id-folder" select="concat('folder',string(ID))" />
      <xsl:variable name="isOpen" select="string(//RequestParameters/Cookie[@Name = $id-folder])" />
      <div class="parent {$isOpen}" id="folder{ID}">
        <div class="title">
          <a>
            <xsl:attribute name="onclick">
              changeUrl('RootFolder=%2F<xsl:value-of select="user:getNameUrl(string(@url))"/>')
            </xsl:attribute>
            <xsl:attribute name="style">
              cursor:pointer
            </xsl:attribute>
            <xsl:value-of select="Title"/>
          </a>
        </div>
        <xsl:if test="FolderChildCount&gt;0 or ItemChildCount&gt;0">
          <div class="child-group">
            <xsl:apply-templates select="//item">
              <xsl:with-param name="level" select="$level+1"/>
              <xsl:with-param name="ParentUrl" select="@url"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
        <div class="button" data-js="site-hierarchy-menu-button"></div>
      </div>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>