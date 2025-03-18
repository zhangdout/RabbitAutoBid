"use client";

import { Button, Dropdown, DropdownDivider, DropdownItem } from "flowbite-react";
import { User } from "next-auth";
import { signOut } from "next-auth/react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import React from "react";
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from "react-icons/ai";
import { HiCog, HiUser } from "react-icons/hi";

type Props = {
  user: User;
};

export default function ({ user }: Props) {
  const router = useRouter();
  return (
    <Dropdown inline label={`Welcome ${user.name}`}>
      <DropdownItem icon={HiUser}>
        <Link href={"/"}>My auctions</Link>
      </DropdownItem>
      <DropdownItem icon={AiFillTrophy}>
        <Link href={"/"}>Auctions won</Link>
      </DropdownItem>
      <DropdownItem icon={AiFillCar}>
        <Link href={"/"}>Sell my car</Link>
      </DropdownItem>
      <DropdownItem icon={HiCog}>
        <Link href={"session"}>Session (dev only!)</Link>
      </DropdownItem>
      <DropdownDivider/>
      <DropdownItem icon={AiOutlineLogout} onClick={() => signOut({redirectTo: '/'})}>
        Sign out
      </DropdownItem>
    </Dropdown>
  );
}
