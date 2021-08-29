from configparser import ConfigParser

def volumes(filename='static.ini', section='volumes'):
    """
    Return os path to diary volume
    :volume: a specific volume from EBA collection
    """
    parser = ConfigParser()
    parser.read(filename)
    
    volumes = {}
    if parser.has_section(section):
        params = parser.items(section)
        for param in params:
            volumes[param[0]] = param[1]
    else:
        raise Exception('Section {} is not found in {}'.format(section, filename))
    return volumes