<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    xpath-default-namespace="http://www.tei-c.org/ns/1.0"
    xmlns:sk="http://www.faculty.washington.edu/ketchley/ns/1.0" xml:lang="en"
    xmlns="http://www.w3.org/1999/xhtml">

    <xsl:template match="/">
        <xsl:variable name="sarah" select="/"/>
        <html>
            <head>
                <title>journal name variants</title>
            </head>
            <body>
                <h2>Names</h2>
                <ul>
                    <xsl:for-each select="distinct-values(//persName/@ref)">
                        <xsl:sort/>
                        <li>
                            <b>
                                <xsl:value-of select="."/>
                            </b>
                            <xsl:text>:</xsl:text>
                            <xsl:variable name="nametobechecked" select="."/>
                            <ul>
                                <xsl:for-each
                                    select="distinct-values($sarah//persName[@ref = $nametobechecked])">
                                    <li>
                                        <xsl:text>“</xsl:text>
                                        <xsl:value-of select="."/>
                                        <xsl:text>”</xsl:text>
                                    </li>
                                </xsl:for-each>
                            </ul>
                        </li>
                    </xsl:for-each>
                </ul>
                <h2>Groups of People</h2>
                <ul>
                    <xsl:for-each select="distinct-values(//personGrp/@ref)">
                        <xsl:sort/>
                        <li>
                            <b>
                                <xsl:value-of select="."/>
                            </b>
                            <xsl:text>:</xsl:text>
                            <xsl:variable name="nametobechecked" select="."/>
                            <ul>
                                <xsl:for-each
                                    select="distinct-values($sarah//personGrp[@ref = $nametobechecked])">
                                    <li>
                                        <xsl:text>“</xsl:text>
                                        <xsl:value-of select="."/>
                                        <xsl:text>”</xsl:text>
                                    </li>
                                </xsl:for-each>
                            </ul>
                        </li>
                    </xsl:for-each>
                </ul>
                <h2>Boats, Hotels</h2>
                <ul>
                    <xsl:for-each select="distinct-values(//name/@ref)">
                        <xsl:sort/> 
                        <li>
                            <b> 
                                <xsl:value-of select="."/> 
                            </b> 
                            <xsl:text>:</xsl:text>
                                <xsl:variable name="nametobechecked" select="."/> 
                            <ul>
                                <xsl:for-each
                                    select="distinct-values($sarah//name[@ref = $nametobechecked][@ref = $nametobechecked])">
                                    <li> 
                                        <xsl:text>“</xsl:text> 
                                        <xsl:value-of select="."/>
                                        <xsl:text>”</xsl:text> 
                                    </li> 
                                </xsl:for-each> 
                            </ul>
                        </li> 
                    </xsl:for-each> 
                </ul>
                <h2>Places</h2>
                <ul>
                    <xsl:for-each select="distinct-values(//placeName/@ref)">
                        <xsl:sort/>
                        <li>
                            <b>
                                <xsl:value-of select="."/>
                            </b>
                            <xsl:text>:</xsl:text>
                            <xsl:variable name="nametobechecked" select="."/>
                            <ul>
                                <xsl:for-each
                                    select="distinct-values($sarah//placeName[@ref = $nametobechecked])">
                                    <li>
                                        <xsl:text>“</xsl:text>
                                        <xsl:value-of select="."/>
                                        <xsl:text>”</xsl:text>
                                    </li>
                                </xsl:for-each>
                            </ul>
                        </li>
                    </xsl:for-each>
                </ul>
            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>
