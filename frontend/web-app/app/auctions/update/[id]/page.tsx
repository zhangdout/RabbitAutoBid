import { getDetailedViewData } from "@/app/actions/auctionActions";
import React from "react";
import AuctionForm from "../../AuctionForm";
import Heading from "@/app/components/Heading";

export default async function Update({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const data = await getDetailedViewData(id);

  return (
    <div className="mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg">
      <Heading
        title="Update your auction"
        subtitle="Please update the details of your car"
      />
      <AuctionForm auction={data} />
    </div>
  );
}
