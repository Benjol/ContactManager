<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:output method="html" version="1.0" indent="yes" />
  
  <xsl:template match="*">
    <html>
      <head>
        <title>Address book</title>
        <link rel="stylesheet" href="addressbook.css" type="text/css"/>
      </head>
      <body>
        <span class="title">Address book</span>
        <table class="contacts">
           <tr>
			        <th class="lastcol">Last name</th>
			        <th class="firstcol">First name</th>
			        <th class="phonecol">Phone</th>
			        <th class="emailcol">Email</th>
           </tr>
           <xsl:apply-templates select="Contacts"/>
        </table>
      </body>
    </html>
	</xsl:template>
  
  <xsl:template match="Contacts">
	  <tr class="row">
		  <td><xsl:value-of select="@Name"/></td>
		  <td><xsl:value-of select="@FirstNames"/></td>
		  <td><xsl:value-of select="@PhoneNumber"/></td>
		  <td>
        <a><xsl:attribute name="href">mailto:<xsl:value-of select="@Email"/></xsl:attribute><xsl:value-of select="@Email"/></a>
        <div class="detail">
          <xsl:if test="Address!=''">
            <div class="address">
              <xsl:value-of select="Address"/>
            </div>
          </xsl:if>
          <table class="people">
            <xsl:apply-templates select="People/Person"/>
          </table>
          <xsl:if test="Notes!=''">
            <div class="notes">
              <xsl:value-of select="Notes"/>
            </div>
          </xsl:if>
        </div>
      </td>
	  </tr>
  </xsl:template>

  <xsl:template match="Person">
	  <tr>
		  <td><xsl:value-of select="@FirstName"/></td>
		  <td><xsl:value-of select="@WorkNumber"/></td>
		  <td><xsl:value-of select="@MobileNumber"/></td>
		  <td><xsl:value-of select="@DateOfBirth"/></td>
		  <td><a><xsl:attribute name="href">mailto:<xsl:value-of select="@Email"/></xsl:attribute><xsl:value-of select="@Email"/></a></td>
	  </tr>
  </xsl:template>
</xsl:stylesheet>