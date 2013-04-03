<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">

  <xsl:output method="xml" indent="yes"/>

  <!-- This is the latest version available -->
  <xsl:variable name="LatestMajor" select="1" />
  <xsl:variable name="LatestMinor" select="0" />
  <xsl:variable name="LatestBuild" select="0" />

  <!-- Match the PluginLocalInfo element created by serialising the data from the category -->
  <xsl:template match="/PluginLocalInfo">
    <UpdateInfos>
      <xsl:variable name="InstalledMajor" select="PluginVersion/@Major" />
      <xsl:variable name="InstalledMinor" select="PluginVersion/@Minor" />
      <xsl:variable name="InstalledBuild" select="PluginVersion/@Build" />
    
      <!-- If we have a new version, add an <UpdateInfo /> element to tell ReSharper a new version is ready -->
      <xsl:if test="($InstalledMajor &lt; $LatestMajor) or ($InstalledMajor = $LatestMajor and $InstalledMinor &lt; $LatestMinor) or ($InstalledMajor = $LatestMajor and $InstalledMinor = $LatestMinor and $InstalledBuild &lt; $LatestBuild)">
        
        <UpdateInfo>
          <!-- TODO: I really should create a release notes page -->
          <InformationUri>http://xunitcontrib.codeplex.com/</InformationUri>
          <Title>
            <xsl:value-of select="concat('xUnit.net test runner ', $LatestMajor, '.', $LatestMinor, '.', $LatestBuild, ' Released')" />
          </Title>
          <Description>A minor upgrade is available.</Description>
          <!-- TODO: It would be nice to have a direct download, but codeplex doesn't really allow that -->
          <DownloadUri>http://xunitcontrib.codeplex.com/</DownloadUri>
          <CompanyName>Matt Ellis</CompanyName>
          <ProductName>xUnit.net test runner</ProductName>
          <ProductVersion><xsl:value-of select="concat($LatestMajor, '.', $LatestMinor, '.', $LatestBuild, '.0')"/></ProductVersion>
          <PriceTag />
          <IsFree>true</IsFree>
        </UpdateInfo>
      
      </xsl:if>
    </UpdateInfos>
  </xsl:template>

</xsl:stylesheet>
