<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:strip-space elements="*"/>
  <xsl:template match="@* | node()" mode="copy-unless-empty">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="AddressBook">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="Contacts">
    <xsl:element name="P">
      <xsl:if test="@IsBusiness!=''">
        <xsl:attribute name="I">
          <xsl:value-of select="@IsBusiness"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@FirstNames!=''">
        <xsl:attribute name="F">
          <xsl:value-of select="@FirstNames"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@Name!=''">
        <xsl:attribute name="N">
          <xsl:value-of select="@Name"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@PhoneNumber!=''">
        <xsl:attribute name="P">
          <xsl:value-of select="@PhoneNumber"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@Email!=''">
        <xsl:attribute name="E">
          <xsl:value-of select="@Email"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="Address">
    <xsl:if test=".!=''">
      <xsl:element name="A">
        <xsl:apply-templates select="node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="People">
    <xsl:if test="node()">
      <xsl:element name="PP">
        <xsl:apply-templates select="node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="Person">
    <xsl:element name="P">
      <xsl:if test="@MobileNumber!=''">
        <xsl:attribute name="M">
          <xsl:value-of select="@MobileNumber"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@LastName!=''">
        <xsl:attribute name="L">
          <xsl:value-of select="@LastName"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@FirstName!=''">
        <xsl:attribute name="F">
          <xsl:value-of select="@FirstName"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@IncludeInDisplayName!=''">
        <xsl:attribute name="I">
          <xsl:value-of select="@IncludeInDisplayName"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@WorkNumber!=''">
        <xsl:attribute name="W">
          <xsl:value-of select="@WorkNumber"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@Email!=''">
        <xsl:attribute name="E">
          <xsl:value-of select="@Email"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="Notes">
    <xsl:if test=".!=''">
      <xsl:element name="N">
        <xsl:apply-templates select="node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="DateOfBirth">
    <xsl:if test=".!=''">
      <xsl:element name="B">
        <xsl:apply-templates select="node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
