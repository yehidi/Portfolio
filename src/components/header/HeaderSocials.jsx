import React from 'react'
import {BsLinkedin, BsFacebook, BsInstagram} from 'react-icons/bs'
import {FaGithub} from 'react-icons/fa'


const HeaderSocials = () => {
  return (
    <div className='header__socials'>
        <a href="https://linkedin.com/in/omriyehidi" target="_blank"><BsLinkedin /></a>
        <a href="https://github.com/yehidi" target="_blank"><FaGithub /></a>
        <a href="http://www.facebook.com/OmriYehidi" target="_blank"><BsFacebook /></a>
        <a href="https://www.instagram.com/omriyehidi" target="_blank"><BsInstagram /></a>

    </div>
  )
}

export default HeaderSocials